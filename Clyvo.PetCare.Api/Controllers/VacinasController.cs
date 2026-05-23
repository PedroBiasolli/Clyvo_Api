using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Vacina;
using Clyvo.PetCare.Api.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.Controllers;

/// <summary>
/// Carteira de vacinação digital — núcleo do cuidado preventivo.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class VacinasController : ControllerBase
{
    private readonly IVacinaService _service;

    public VacinasController(IVacinaService service) => _service = service;

    /// <summary>Lista vacinas registradas com filtro por nome.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<VacinaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<VacinaResponseDto>>> Listar(
        [FromQuery] string? nome,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20)
    {
        var resultado = await _service.ListarAsync(nome, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    /// <summary>Busca uma vacina pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VacinaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VacinaResponseDto>> BuscarPorId(int id)
    {
        var vacina = await _service.BuscarPorIdAsync(id);
        return vacina is null ? NotFound(new { mensagem = $"Vacina {id} não encontrada." }) : Ok(vacina);
    }

    /// <summary>Lista a carteira de vacinação completa de um pet.</summary>
    [HttpGet("pet/{petId:int}")]
    [ProducesResponseType(typeof(IEnumerable<VacinaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VacinaResponseDto>>> BuscarPorPet(int petId)
    {
        var vacinas = await _service.BuscarPorPetAsync(petId);
        return Ok(vacinas);
    }

    /// <summary>Lista doses cuja DataProximaDose já passou (atrasadas).</summary>
    [HttpGet("atrasadas")]
    [ProducesResponseType(typeof(IEnumerable<VacinaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VacinaResponseDto>>> BuscarDosesAtrasadas()
    {
        var vacinas = await _service.BuscarDosesAtrasadasAsync();
        return Ok(vacinas);
    }

    /// <summary>Lista próximas doses agendadas para os próximos N dias.</summary>
    [HttpGet("proximas/{diasAFrente:int}")]
    [ProducesResponseType(typeof(IEnumerable<VacinaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VacinaResponseDto>>> BuscarProximasDoses(int diasAFrente)
    {
        var vacinas = await _service.BuscarProximasDosesAsync(diasAFrente);
        return Ok(vacinas);
    }

    /// <summary>Registra uma nova aplicação de vacina na carteira de um pet.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VacinaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VacinaResponseDto>> Criar([FromBody] CriarVacinaDto dto)
    {
        try
        {
            var criada = await _service.CriarAsync(dto);
            if (criada is null)
                return BadRequest(new { mensagem = "Pet ou veterinário não encontrado." });

            return CreatedAtAction(nameof(BuscarPorId), new { id = criada.Id }, criada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Atualiza um registro de vacina.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(VacinaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VacinaResponseDto>> Atualizar(int id, [FromBody] AtualizarVacinaDto dto)
    {
        var atualizada = await _service.AtualizarAsync(id, dto);
        return atualizada is null ? NotFound(new { mensagem = $"Vacina {id} não encontrada." }) : Ok(atualizada);
    }

    /// <summary>Remove um registro de vacina.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id)
    {
        var removida = await _service.RemoverAsync(id);
        return removida ? NoContent() : NotFound(new { mensagem = $"Vacina {id} não encontrada." });
    }
}
