namespace Clyvo.PetCare.Api.Hateoas;

/// <summary>Implementação baseada no <see cref="LinkGenerator"/> do roteamento do ASP.NET Core.</summary>
public sealed class LinkService : ILinkService
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LinkService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Gerar(string acao, string controlador, object? valores = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        return httpContext is null
            ? _linkGenerator.GetPathByAction(acao, controlador, valores)
            : _linkGenerator.GetUriByAction(httpContext, acao, controlador, valores);
    }
}
