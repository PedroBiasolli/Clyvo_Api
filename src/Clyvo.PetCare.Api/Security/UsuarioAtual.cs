using Clyvo.PetCare.Api.Middlewares;
using Clyvo.PetCare.Application.Abstractions;
using Clyvo.PetCare.Infrastructure.Security;

namespace Clyvo.PetCare.Api.Security;

/// <summary>Expõe à camada de aplicação quem está autenticado na requisição atual.</summary>
public sealed class UsuarioAtual : IUsuarioAtual
{
    private const string Anonimo = "anonimo";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioAtual(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public string Identificador
        => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimsClyvo.Email)?.Value ?? Anonimo;

    public string? CorrelationId
        => _httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.ItemKey] as string;
}
