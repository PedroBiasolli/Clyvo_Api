using Clyvo.PetCare.Api.Middlewares;
using Clyvo.PetCare.Application.Abstractions;

namespace Clyvo.PetCare.Api.Security;

/// <summary>Expõe à camada de aplicação quem está autenticado na requisição atual.</summary>
public sealed class UsuarioAtual : IUsuarioAtual
{
    private const string Anonimo = "anonimo";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioAtual(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public int? Id
        => int.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimsClyvo.Id)?.Value, out var id) ? id : null;

    public string Identificador
        => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimsClyvo.Email)?.Value ?? Anonimo;

    public string? CorrelationId
        => _httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.ItemKey] as string;
}
