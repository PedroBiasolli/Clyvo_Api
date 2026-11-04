using Clyvo.PetCare.Api.Security;
using Clyvo.PetCare.Application.Abstractions;
using Clyvo.PetCare.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clyvo.PetCare.Api.Configuration;

public static class SegurancaConfiguration
{
    /// <summary>Autenticação JWT Bearer + políticas de autorização por perfil.</summary>
    public static IServiceCollection AddSegurancaJwt(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUsuarioAtual, UsuarioAtual>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        // Configuração lazy: lê JwtSettings do contêiner (mesma fonte usada para emitir o token).
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = ChaveJwt.Criar(jwt.ChaveSecreta),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = ClaimsClyvo.Nome,
                    RoleClaimType = ClaimsClyvo.Perfil
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Politicas.Cadastro, policy => policy.RequireRole(Politicas.PerfisCadastro))
            .AddPolicy(Politicas.Clinica, policy => policy.RequireRole(Politicas.PerfisClinica))
            .AddPolicy(Politicas.Administracao, policy => policy.RequireRole(Politicas.PerfisAdministracao));

        return services;
    }
}
