using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Consulta;
using Clyvo.PetCare.Api.Application.Services.Interfaces;
using Clyvo.PetCare.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.Controllers;

/// <summary>
/// Consultas veterinárias — núcleo da continuidade do cuidado.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ConsultasController : ControllerBase
{
    private readonly IConsultaService _service;

    public ConsultasController(IConsultaService service) => _service = service;

    /// <summary>Lista consultas com filtros (tipo, status, período).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ConsultaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<ConsultaResponseDto>>> Listar(
        [FromQuery] TipoConsulta? tipo,
        [FromQuery] StatusConsulta? status,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20)
    {
        var resultado = await _service.ListarAsync(tipo, status, dataInicio, dataFim, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    /// <summary>Busca uma consulta pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ConsultaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultaResponseDto>> BuscarPorId(int id)
    {
        var consulta = await _service.BuscarPorIdAsync(id);
        return consulta is null ? NotFound(new { mensagem = $"Consulta {id} não encontrada." }) : Ok(consulta);
    }

    /// <summary>Lista o histórico de consultas de um pet específico.</summary>
    [HttpGet("pet/{petId:int}")]
    [ProducesResponseType(typeof(IEnumerable<ConsultaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConsultaResponseDto>>> BuscarPorPet(int petId)
    {
        var consultas = await _service.BuscarPorPetAsync(petId);
        return Ok(consultas);
    }

    /// <summary>
    /// Lista retornos sugeridos para os próximos N dias.
    /// Endpoint que alimenta o motor de lembretes preventivos.
    /// </summary>
    [HttpGet("retornos-proximos/{diasAFrente:int}")]
    [ProducesResponseType(typeof(IEnumerable<ConsultaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConsultaResponseDto>>> BuscarRetornosProximos(int diasAFrente)
    {
        var consultas = await _service.BuscarRetornosProximosAsync(diasAFrente);
        return Ok(consultas);
    }

    /// <summary>Agenda uma nova consulta.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConsultaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConsultaResponseDto>> Criar([FromBody] CriarConsultaDto dto)
    {
        var criada = await _service.CriarAsync(dto);
        if (criada is null)
            return BadRequest(new { mensagem = "Pet ou veterinário não encontrado/inativo." });

        return CreatedAtAction(nameof(BuscarPorId), new { id = criada.Id }, criada);
    }

    /// <summary>Atualiza uma consulta (status, diagnóstico, prescrição, retorno).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ConsultaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultaResponseDto>> Atualizar(int id, [FromBody] AtualizarConsultaDto dto)
    {
        var atualizada = await _service.AtualizarAsync(id, dto);
        return atualizada is null ? NotFound(new { mensagem = $"Consulta {id} não encontrada." }) : Ok(atualizada);
    }

    /// <summary>Remove uma consulta. Realizadas não podem ser removidas (apenas canceladas).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id)
    {
        try
        {
            var removida = await _service.RemoverAsync(id);
            return removida ? NoContent() : NotFound(new { mensagem = $"Consulta {id} não encontrada." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}
