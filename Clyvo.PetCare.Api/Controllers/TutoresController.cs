using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Tutor;
using Clyvo.PetCare.Api.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.Controllers;

/// <summary>
/// Gestão de tutores (responsáveis legais pelos pets).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class TutoresController : ControllerBase
{
    private readonly ITutorService _service;

    public TutoresController(ITutorService service) => _service = service;

    /// <summary>
    /// Lista tutores com filtros opcionais e paginação.
    /// </summary>
    /// <param name="nome">Filtro por nome (busca parcial, case-insensitive).</param>
    /// <param name="cidade">Filtro por cidade exata.</param>
    /// <param name="pagina">Página (default 1).</param>
    /// <param name="tamanhoPagina">Itens por página (1–100, default 20).</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TutorResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<TutorResponseDto>>> Listar(
        [FromQuery] string? nome,
        [FromQuery] string? cidade,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20)
    {
        var resultado = await _service.ListarAsync(nome, cidade, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    /// <summary>Busca um tutor pelo seu identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TutorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TutorResponseDto>> BuscarPorId(int id)
    {
        var tutor = await _service.BuscarPorIdAsync(id);
        return tutor is null ? NotFound(new { mensagem = $"Tutor {id} não encontrado." }) : Ok(tutor);
    }

    /// <summary>Busca um tutor pelo CPF.</summary>
    [HttpGet("cpf/{cpf}")]
    [ProducesResponseType(typeof(TutorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TutorResponseDto>> BuscarPorCpf(string cpf)
    {
        var tutor = await _service.BuscarPorCpfAsync(cpf);
        return tutor is null ? NotFound(new { mensagem = $"Tutor com CPF {cpf} não encontrado." }) : Ok(tutor);
    }

    /// <summary>Cadastra um novo tutor.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TutorResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TutorResponseDto>> Criar([FromBody] CriarTutorDto dto)
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

    /// <summary>Atualiza os dados de um tutor existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TutorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TutorResponseDto>> Atualizar(int id, [FromBody] AtualizarTutorDto dto)
    {
        var atualizado = await _service.AtualizarAsync(id, dto);
        return atualizado is null ? NotFound(new { mensagem = $"Tutor {id} não encontrado." }) : Ok(atualizado);
    }

    /// <summary>Remove um tutor. Falha se ele tiver pets cadastrados.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id)
    {
        try
        {
            var removido = await _service.RemoverAsync(id);
            return removido ? NoContent() : NotFound(new { mensagem = $"Tutor {id} não encontrado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}
