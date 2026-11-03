using Clyvo.PetCare.Api.Hateoas;
using Clyvo.PetCare.Application.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.Controllers;

/// <summary>
/// Base dos controllers REST: exige autenticação por padrão, documenta erros
/// como ProblemDetails e oferece os helpers de HATEOAS (links de item e de paginação).
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(ProblemDetails))]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public abstract class ApiControllerBase : ControllerBase
{
    protected ApiControllerBase(ILinkService links) => Links = links;

    protected ILinkService Links { get; }

    /// <summary>Adiciona um link HATEOAS ao recurso (ignorado se a rota não for resolvida).</summary>
    protected void AdicionarLink(RecursoDto recurso, string rel, string metodo, string acao, string controlador, object? valores = null)
    {
        var href = Links.Gerar(acao, controlador, valores);
        if (href is not null)
            recurso.Links.Add(new LinkDto(href, rel, metodo));
    }

    /// <summary>
    /// Links de navegação da página (self, primeira, anterior, próxima, última),
    /// preservando os filtros e a ordenação informados na query string.
    /// </summary>
    protected PagedResultDto<T> ComLinksDePaginacao<T>(PagedResultDto<T> pagina, string acao, string controlador, object filtros)
    {
        void AdicionarPagina(string rel, int numero)
        {
            var valores = QueryStringHelper.ParaValoresDeRota(filtros);
            valores["pagina"] = numero;
            AdicionarLink(pagina, rel, HttpMethods.Get, acao, controlador, valores);
        }

        AdicionarPagina("self", pagina.Pagina);
        AdicionarPagina("primeira-pagina", 1);

        if (pagina.TemPaginaAnterior)
            AdicionarPagina("pagina-anterior", pagina.Pagina - 1);

        if (pagina.TemProximaPagina)
            AdicionarPagina("proxima-pagina", pagina.Pagina + 1);

        if (pagina.TotalPaginas > 0)
            AdicionarPagina("ultima-pagina", pagina.TotalPaginas);

        return pagina;
    }

    /// <summary>Envolve uma lista não paginada num recurso com link "self".</summary>
    protected ColecaoDto<T> ComoColecao<T>(IReadOnlyList<T> itens, string acao, string controlador, object? valores = null)
    {
        var colecao = new ColecaoDto<T> { Itens = itens };
        AdicionarLink(colecao, "self", HttpMethods.Get, acao, controlador, valores);
        return colecao;
    }
}
