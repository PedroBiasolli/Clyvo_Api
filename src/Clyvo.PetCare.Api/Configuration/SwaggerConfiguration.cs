using System.Reflection;
using Clyvo.PetCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Clyvo.PetCare.Api.Configuration;

public static class SwaggerConfiguration
{
    private const string EsquemaBearer = "Bearer";

    public static IServiceCollection AddDocumentacaoSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Clyvo Pet Care API",
                Version = "v1",
                Description =
                    "API REST para continuidade do cuidado e engajamento na jornada de saúde de pets. " +
                    "Sprint 4 — Clean Architecture, JWT, HATEOAS, paginação/ordenação/filtros, " +
                    "Oracle (EF Core) + MongoDB (auditoria), observabilidade e ProblemDetails.\n\n" +
                    "**Como autenticar:** faça `POST /api/v1/auth/login`, copie o `accessToken` " +
                    "e clique em **Authorize** informando apenas o token.",
                Contact = new OpenApiContact { Name = "Equipe Clyvo Pet Care", Email = "contato@clyvo.com.br" },
                License = new OpenApiLicense { Name = "Uso acadêmico — FIAP" }
            });

            // Comentários XML da API (controllers) e da Application (DTOs e filtros).
            IncluirComentariosXml(c, typeof(Program).Assembly);
            IncluirComentariosXml(c, typeof(ITutorService).Assembly);

            c.EnableAnnotations();
            c.DescribeAllParametersInCamelCase();

            c.AddSecurityDefinition(EsquemaBearer, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Informe o token JWT obtido em /api/v1/auth/login."
            });

            c.OperationFilter<AutorizacaoOperationFilter>();
        });

        return services;
    }

    private static void IncluirComentariosXml(SwaggerGenOptions options, Assembly assembly)
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
        if (File.Exists(caminho))
            options.IncludeXmlComments(caminho, includeControllerXmlComments: true);
    }

    /// <summary>
    /// Marca no Swagger apenas os endpoints protegidos (cadeado + respostas 401/403),
    /// deixando login, health e métricas como públicos.
    /// </summary>
    private sealed class AutorizacaoOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var metadados = context.ApiDescription.ActionDescriptor.EndpointMetadata;

            var exigeAutenticacao = metadados.OfType<IAuthorizeData>().Any()
                                    && !metadados.OfType<IAllowAnonymous>().Any();
            if (!exigeAutenticacao)
                return;

            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Token ausente, inválido ou expirado." });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Perfil sem permissão para esta operação." });

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = EsquemaBearer }
                    }] = Array.Empty<string>()
                }
            };
        }
    }
}
