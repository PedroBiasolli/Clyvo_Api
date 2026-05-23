using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Pet;
using Clyvo.PetCare.Api.Application.Services.Interfaces;
using Clyvo.PetCare.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.Controllers;

/// <summary>
/// Gestão de pets (foco em cachorros nesta versão).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PetsController : ControllerBase
{
    private readonly IPetService _service;

    public PetsController(IPetService service) => _service = service;

    /// <summary>Lista pets com filtros opcionais e paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<PetResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<PetResponseDto>>> Listar(
        [FromQuery] string? nome,
        [FromQuery] string? raca,
        [FromQuery] PorteCachorro? porte,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20)
    {
        var resultado = await _service.ListarAsync(nome, raca, porte, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    /// <summary>Busca um pet pelo seu identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetResponseDto>> BuscarPorId(int id)
    {
        var pet = await _service.BuscarPorIdAsync(id);
        return pet is null ? NotFound(new { mensagem = $"Pet {id} não encontrado." }) : Ok(pet);
    }

    /// <summary>Lista todos os pets de um tutor específico.</summary>
    [HttpGet("tutor/{tutorId:int}")]
    [ProducesResponseType(typeof(IEnumerable<PetResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PetResponseDto>>> BuscarPorTutor(int tutorId)
    {
        var pets = await _service.BuscarPorTutorAsync(tutorId);
        return Ok(pets);
    }

    /// <summary>Busca um pet pelo número do microchip.</summary>
    [HttpGet("microchip/{microchip}")]
    [ProducesResponseType(typeof(PetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetResponseDto>> BuscarPorMicrochip(string microchip)
    {
        var pet = await _service.BuscarPorMicrochipAsync(microchip);
        return pet is null ? NotFound(new { mensagem = $"Nenhum pet encontrado com o microchip {microchip}." }) : Ok(pet);
    }

    /// <summary>Cadastra um novo pet vinculado a um tutor.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PetResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PetResponseDto>> Criar([FromBody] CriarPetDto dto)
    {
        try
        {
            var criado = await _service.CriarAsync(dto);
            if (criado is null)
                return BadRequest(new { mensagem = $"Tutor {dto.TutorId} não encontrado." });

            return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Atualiza os dados de um pet (não altera tutor, sexo ou nascimento).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetResponseDto>> Atualizar(int id, [FromBody] AtualizarPetDto dto)
    {
        var atualizado = await _service.AtualizarAsync(id, dto);
        return atualizado is null ? NotFound(new { mensagem = $"Pet {id} não encontrado." }) : Ok(atualizado);
    }

    /// <summary>Remove um pet. Falha se houver histórico clínico.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id)
    {
        try
        {
            var removido = await _service.RemoverAsync(id);
            return removido ? NoContent() : NotFound(new { mensagem = $"Pet {id} não encontrado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}
