using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Veterinario;
using Clyvo.PetCare.Api.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.Controllers;

/// <summary>
/// Gestão de veterinários cadastrados na plataforma.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class VeterinariosController : ControllerBase
{
    private readonly IVeterinarioService _service;

    public VeterinariosController(IVeterinarioService service) => _service = service;

    /// <summary>Lista veterinários com filtros opcionais.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<VeterinarioResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<VeterinarioResponseDto>>> Listar(
        [FromQuery] string? especialidade,
        [FromQuery] bool? ativo,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20)
    {
        var resultado = await _service.ListarAsync(especialidade, ativo, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    /// <summary>Busca um veterinário pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VeterinarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VeterinarioResponseDto>> BuscarPorId(int id)
    {
        var vet = await _service.BuscarPorIdAsync(id);
        return vet is null ? NotFound(new { mensagem = $"Veterinário {id} não encontrado." }) : Ok(vet);
    }

    /// <summary>Busca um veterinário pelo CRMV.</summary>
    [HttpGet("crmv/{crmv}")]
    [ProducesResponseType(typeof(VeterinarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VeterinarioResponseDto>> BuscarPorCrmv(string crmv)
    {
        var vet = await _service.BuscarPorCrmvAsync(crmv);
        return vet is null ? NotFound(new { mensagem = $"Veterinário com CRMV {crmv} não encontrado." }) : Ok(vet);
    }

    /// <summary>Cadastra um novo veterinário.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VeterinarioResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VeterinarioResponseDto>> Criar([FromBody] CriarVeterinarioDto dto)
    {
        try
        {
            var criado = await _service.CriarAsync(dto);
            return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Atualiza dados de um veterinário existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(VeterinarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VeterinarioResponseDto>> Atualizar(int id, [FromBody] AtualizarVeterinarioDto dto)
    {
        var atualizado = await _service.AtualizarAsync(id, dto);
        return atualizado is null ? NotFound(new { mensagem = $"Veterinário {id} não encontrado." }) : Ok(atualizado);
    }

    /// <summary>Remove um veterinário. Falha se ele tiver consultas vinculadas.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id)
    {
        try
        {
            var removido = await _service.RemoverAsync(id);
            return removido ? NoContent() : NotFound(new { mensagem = $"Veterinário {id} não encontrado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}
