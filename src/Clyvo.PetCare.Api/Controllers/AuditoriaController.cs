using Clyvo.PetCare.Api.Hateoas;
using Clyvo.PetCare.Api.Security;
using Clyvo.PetCare.Application.DTOs.Auditoria;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.Controllers;

/// <summary>
/// Trilha de auditoria (MongoDB): quem criou, alterou ou removeu cada registro,
/// com o snapshot do dado. Acesso restrito a administradores.
/// </summary>
[Route("api/v1/auditoria")]
[Authorize(Policy = Politicas.Administracao)]
public sealed class AuditoriaController : ApiControllerBase
{
    public const string Nome = "Auditoria";

    private readonly IAuditoriaConsultaService _service;

    public AuditoriaController(IAuditoriaConsultaService service, ILinkService links) : base(links) => _service = service;

    /// <summary>Lista eventos com filtros (entidade, id, ação, usuário, período) e paginação; mais recentes primeiro.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<RegistroAuditoriaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PagedResultDto<RegistroAuditoriaDto>>> Listar(
        [FromQuery] AuditoriaQueryDto query, CancellationToken cancellationToken)
    {
        var pagina = await _service.ListarAsync(query, cancellationToken);

        foreach (var registro in pagina.Itens)
            AdicionarLink(registro, "self", HttpMethods.Get, nameof(BuscarPorId), Nome, new { id = registro.Id });

        return Ok(ComLinksDePaginacao(pagina, nameof(Listar), Nome, query));
    }

    /// <summary>Busca um evento de auditoria pelo identificador (ObjectId).</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RegistroAuditoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistroAuditoriaDto>> BuscarPorId(string id, CancellationToken cancellationToken)
    {
        var registro = await _service.BuscarPorIdAsync(id, cancellationToken);
        AdicionarLink(registro, "self", HttpMethods.Get, nameof(BuscarPorId), Nome, new { id });
        AdicionarLink(registro, "historico-da-entidade", HttpMethods.Get, nameof(Listar), Nome,
            new { entidade = registro.Entidade, entidadeId = registro.EntidadeId });
        return Ok(registro);
    }

    /// <summary>Totais de operações por entidade e ação (aggregation pipeline do MongoDB).</summary>
    /// <param name="desde">Considera apenas eventos a partir desta data (UTC).</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    [HttpGet("resumo")]
    [ProducesResponseType(typeof(IReadOnlyList<ResumoAuditoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResumoAuditoriaDto>>> Resumo([FromQuery] DateTime? desde, CancellationToken cancellationToken)
        => Ok(await _service.ResumirAsync(desde, cancellationToken));
}
