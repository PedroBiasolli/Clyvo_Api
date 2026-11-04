using System.Net.Http.Headers;
using System.Net.Http.Json;
using Clyvo.PetCare.Application.Abstractions;
using Clyvo.PetCare.Application.DTOs.Auth;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Repositories;
using Clyvo.PetCare.Infrastructure.NoSql;
using Clyvo.PetCare.Infrastructure.Persistence;
using Clyvo.PetCare.Infrastructure.Persistence.Seed;
using Clyvo.PetCare.IntegrationTests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Clyvo.PetCare.IntegrationTests.Fixtures;

/// <summary>
/// Host de teste da API completa. Troca apenas as dependências externas:
/// Oracle → EF Core InMemory e MongoDB → repositório em memória. Todo o resto
/// (JWT real, autorização, HATEOAS, ProblemDetails, health checks) é o de produção.
/// </summary>
public class ClyvoWebApplicationFactory : WebApplicationFactory<Program>
{
    // Mesmos valores da seção SeedAdmin do appsettings.json.
    public const string EmailAdmin = "admin@clyvo.com.br";
    public const string SenhaAdmin = "Clyvo@2026";
    public const string SenhaPadraoEquipe = "Equipe@2026";

    private readonly string _databaseName = $"clyvo-tests-{Guid.NewGuid()}";

    public InMemoryAuditoriaRepository Auditoria { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Oracle → InMemory (remove também as options para não haver dois providers).
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.EnableSensitiveDataLogging();
            });

            // MongoDB → repositório em memória (sem servidor Mongo nos testes).
            services.RemoveAll<IAuditoriaRepository>();
            services.AddSingleton<IAuditoriaRepository>(Auditoria);

            // Tarefas de inicialização que acessariam bancos reais: o seed é feito por ResetDatabase().
            RemoverHostedService<AdminSeedHostedService>(services);
            RemoverHostedService<MongoIndicesHostedService>(services);

            // O check do Mongo vira "saudável" — ele é coberto pelo repositório fake.
            services.PostConfigure<HealthCheckServiceOptions>(options =>
            {
                foreach (var registro in options.Registrations.Where(r => r.Name == "mongodb"))
                    registro.Factory = _ => new MongoSimuladoHealthCheck();
            });

            // Atenção: nunca chamar services.BuildServiceProvider() aqui — congelaria o
            // logger do Serilog antes do host real subir ("The logger is already frozen").
        });
    }

    /// <summary>Banco limpo + administrador padrão + auditoria vazia para cada teste.</summary>
    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();

        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        contexto.Database.EnsureDeleted();
        contexto.Database.EnsureCreated();

        scope.ServiceProvider.GetRequiredService<UsuarioAdminSeeder>()
            .GarantirAdministradorAsync().GetAwaiter().GetResult();

        Auditoria.Limpar();
    }

    /// <summary>Cliente HTTP já autenticado (JWT real obtido em /auth/login) com o perfil desejado.</summary>
    public async Task<HttpClient> CriarClienteAutenticadoAsync(PerfilUsuario perfil = PerfilUsuario.Admin)
    {
        var (email, senha) = perfil == PerfilUsuario.Admin
            ? (EmailAdmin, SenhaAdmin)
            : await GarantirUsuarioAsync(perfil);

        var cliente = CreateClient();
        var token = await ObterTokenAsync(cliente, email, senha);
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        return cliente;
    }

    public static async Task<TokenDto> ObterTokenAsync(HttpClient cliente, string email, string senha)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/v1/auth/login", new LoginDto { Email = email, Senha = senha });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<TokenDto>(JsonTestOptions.Default))!;
    }

    private async Task<(string Email, string Senha)> GarantirUsuarioAsync(PerfilUsuario perfil)
    {
        var email = $"{perfil.ToString().ToLowerInvariant()}@clyvo.com.br";

        using var scope = Services.CreateScope();
        var usuarios = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

        if (!await usuarios.ExisteEmailAsync(email))
        {
            var hash = scope.ServiceProvider.GetRequiredService<IHashSenha>().Gerar(SenhaPadraoEquipe);
            await usuarios.AdicionarAsync(Usuario.Criar($"Equipe {perfil}", email, hash, perfil));
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync();
        }

        return (email, SenhaPadraoEquipe);
    }

    private static void RemoverHostedService<THostedService>(IServiceCollection services)
        where THostedService : class, IHostedService
    {
        var descritores = services
            .Where(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(THostedService))
            .ToList();

        foreach (var descritor in descritores)
            services.Remove(descritor);
    }

    private sealed class MongoSimuladoHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(HealthCheckResult.Healthy("MongoDB simulado em memória."));
    }
}
