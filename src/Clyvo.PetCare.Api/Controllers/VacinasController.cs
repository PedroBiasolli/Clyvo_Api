using Clyvo.PetCare.Api.Hateoas;
using Clyvo.PetCare.Api.Security;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Vacina;
using Clyvo.PetCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.Controllers;

/// <summary>Carteira de vacinação digital dos pets.</summary>
[Route("api/v1/vacinas")]
public sealed class VacinasController : ApiControllerBase
{
    public const string Nome = "Vacinas";

    private readonly IVacinaService _service;

    public VacinasController(IVacinaService service, ILinkService links) : base(links) => _service = service;

    /// <summary>Lista registros de vacina com filtros (nome, pet), ordenação e paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<VacinaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<VacinaResponseDto>>> Listar(
        [FromQuery] VacinaQueryDto query, CancellationToken cancellationToken)
    {
        var pagina = await _service.ListarAsync(query, cancellationToken);

        foreach (var vacina in pagina.Itens)
            ComLinks(vacina);

        return Ok(ComLinksDePaginacao(pagina, nameof(Listar), Nome, query));
    }

    /// <summary>Busca um registro de vacina pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VacinaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VacinaResponseDto>> BuscarPorId(int id, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.BuscarPorIdAsync(id, cancellationToken)));

    /// <summary>Carteira de vacinação completa de um pet.</summary>
    [HttpGet("pet/{petId:int}")]
    [ProducesResponseType(typeof(ColecaoDto<VacinaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ColecaoDto<VacinaResponseDto>>> BuscarPorPet(int petId, CancellationToken cancellationToken)
    {
        var vacinas = await _service.BuscarPorPetAsync(petId, cancellationToken);

        foreach (var vacina in vacinas)
            ComLinks(vacina);

        return Ok(ComoColecao(vacinas, nameof(BuscarPorPet), Nome, new { petId }));
    }

    /// <summary>Doses vencidas (próxima dose anterior a hoje) — prioridade do motor preventivo.</summary>
    [HttpGet("atrasadas")]
    [ProducesResponseType(typeof(ColecaoDto<VacinaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ColecaoDto<VacinaResponseDto>>> BuscarDosesAtrasadas(CancellationToken cancellationToken)
    {
        var vacinas = await _service.BuscarDosesAtrasadasAsync(cancellationToken);

        foreach (var vacina in vacinas)
            ComLinks(vacina);

        return Ok(ComoColecao(vacinas, nameof(BuscarDosesAtrasadas), Nome));
    }

    /// <summary>Próximas doses previstas para os próximos N dias (1 a 365).</summary>
    [HttpGet("proximas/{diasAFrente:int}")]
    [ProducesResponseType(typeof(ColecaoDto<VacinaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ColecaoDto<VacinaResponseDto>>> BuscarProximasDoses(int diasAFrente, CancellationToken cancellationToken)
    {
        var vacinas = await _service.BuscarProximasDosesAsync(diasAFrente, cancellationToken);

        foreach (var vacina in vacinas)
            ComLinks(vacina);

        return Ok(ComoColecao(vacinas, nameof(BuscarProximasDoses), Nome, new { diasAFrente }));
    }

    /// <summary>Registra a aplicação de uma vacina (Admin/Veterinário).</summary>
    [HttpPost]
    [Authorize(Policy = Politicas.Clinica)]
    [ProducesResponseType(typeof(VacinaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<VacinaResponseDto>> Criar([FromBody] CriarVacinaDto dto, CancellationToken cancellationToken)
    {
        var criada = ComLinks(await _service.CriarAsync(dto, cancellationToken));
        return CreatedAtAction(nameof(BuscarPorId), new { id = criada.Id }, criada);
    }

    /// <summary>Corrige um registro de vacina (Admin/Veterinário).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.Clinica)]
    [ProducesResponseType(typeof(VacinaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<VacinaResponseDto>> Atualizar(int id, [FromBody] AtualizarVacinaDto dto, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.AtualizarAsync(id, dto, cancellationToken)));

    /// <summary>Remove um registro de vacina (somente Admin).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.Administracao)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        await _service.RemoverAsync(id, cancellationToken);
        return NoContent();
    }

    private VacinaResponseDto ComLinks(VacinaResponseDto vacina)
    {
        var rota = new { id = vacina.Id };
        AdicionarLink(vacina, "self", HttpMethods.Get, nameof(BuscarPorId), Nome, rota);
        AdicionarLink(vacina, "atualizar", HttpMethods.Put, nameof(Atualizar), Nome, rota);
        AdicionarLink(vacina, "remover", HttpMethods.Delete, nameof(Remover), Nome, rota);
        AdicionarLink(vacina, "pet", HttpMethods.Get, nameof(PetsController.BuscarPorId), PetsController.Nome, new { id = vacina.PetId });

        if (vacina.VeterinarioId.HasValue)
            AdicionarLink(vacina, "veterinario", HttpMethods.Get, nameof(VeterinariosController.BuscarPorId), VeterinariosController.Nome, new { id = vacina.VeterinarioId.Value });

        return vacina;
    }
}
