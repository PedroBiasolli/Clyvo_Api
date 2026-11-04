using Clyvo.PetCare.Api.Hateoas;
using Clyvo.PetCare.Api.Security;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Veterinario;
using Clyvo.PetCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.Controllers;

/// <summary>Gestão do corpo clínico (veterinários).</summary>
[Route("api/v1/veterinarios")]
public sealed class VeterinariosController : ApiControllerBase
{
    public const string Nome = "Veterinarios";

    private readonly IVeterinarioService _service;

    public VeterinariosController(IVeterinarioService service, ILinkService links) : base(links) => _service = service;

    /// <summary>Lista veterinários com filtros (nome, especialidade, ativo), ordenação e paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<VeterinarioResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<VeterinarioResponseDto>>> Listar(
        [FromQuery] VeterinarioQueryDto query, CancellationToken cancellationToken)
    {
        var pagina = await _service.ListarAsync(query, cancellationToken);

        foreach (var veterinario in pagina.Itens)
            ComLinks(veterinario);

        return Ok(ComLinksDePaginacao(pagina, nameof(Listar), Nome, query));
    }

    /// <summary>Busca um veterinário pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VeterinarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VeterinarioResponseDto>> BuscarPorId(int id, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.BuscarPorIdAsync(id, cancellationToken)));

    /// <summary>Busca um veterinário pelo CRMV.</summary>
    [HttpGet("crmv/{crmv}")]
    [ProducesResponseType(typeof(VeterinarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VeterinarioResponseDto>> BuscarPorCrmv(string crmv, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.BuscarPorCrmvAsync(crmv, cancellationToken)));

    /// <summary>Cadastra um veterinário (somente Admin).</summary>
    [HttpPost]
    [Authorize(Policy = Politicas.Administracao)]
    [ProducesResponseType(typeof(VeterinarioResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<VeterinarioResponseDto>> Criar([FromBody] CriarVeterinarioDto dto, CancellationToken cancellationToken)
    {
        var criado = ComLinks(await _service.CriarAsync(dto, cancellationToken));
        return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza um veterinário — inclusive ativar/inativar (somente Admin).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.Administracao)]
    [ProducesResponseType(typeof(VeterinarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<VeterinarioResponseDto>> Atualizar(int id, [FromBody] AtualizarVeterinarioDto dto, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.AtualizarAsync(id, dto, cancellationToken)));

    /// <summary>Remove um veterinário sem consultas vinculadas (somente Admin).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.Administracao)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        await _service.RemoverAsync(id, cancellationToken);
        return NoContent();
    }

    private VeterinarioResponseDto ComLinks(VeterinarioResponseDto veterinario)
    {
        var rota = new { id = veterinario.Id };
        AdicionarLink(veterinario, "self", HttpMethods.Get, nameof(BuscarPorId), Nome, rota);
        AdicionarLink(veterinario, "atualizar", HttpMethods.Put, nameof(Atualizar), Nome, rota);
        AdicionarLink(veterinario, "remover", HttpMethods.Delete, nameof(Remover), Nome, rota);
        AdicionarLink(veterinario, "consultas", HttpMethods.Get, nameof(ConsultasController.Listar), ConsultasController.Nome, new { veterinarioId = veterinario.Id });
        return veterinario;
    }
}
