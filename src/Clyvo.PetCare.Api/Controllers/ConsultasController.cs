using Clyvo.PetCare.Api.Hateoas;
using Clyvo.PetCare.Api.Security;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Consulta;
using Clyvo.PetCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.Controllers;

/// <summary>Consultas veterinárias — núcleo da continuidade do cuidado.</summary>
[Route("api/v1/consultas")]
public sealed class ConsultasController : ApiControllerBase
{
    public const string Nome = "Consultas";

    private readonly IConsultaService _service;

    public ConsultasController(IConsultaService service, ILinkService links) : base(links) => _service = service;

    /// <summary>Lista consultas com filtros (tipo, status, pet, veterinário, período), ordenação e paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ConsultaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PagedResultDto<ConsultaResponseDto>>> Listar(
        [FromQuery] ConsultaQueryDto query, CancellationToken cancellationToken)
    {
        var pagina = await _service.ListarAsync(query, cancellationToken);

        foreach (var consulta in pagina.Itens)
            ComLinks(consulta);

        return Ok(ComLinksDePaginacao(pagina, nameof(Listar), Nome, query));
    }

    /// <summary>Busca uma consulta pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ConsultaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultaResponseDto>> BuscarPorId(int id, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.BuscarPorIdAsync(id, cancellationToken)));

    /// <summary>Histórico de consultas de um pet (mais recente primeiro).</summary>
    [HttpGet("pet/{petId:int}")]
    [ProducesResponseType(typeof(ColecaoDto<ConsultaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ColecaoDto<ConsultaResponseDto>>> BuscarPorPet(int petId, CancellationToken cancellationToken)
    {
        var consultas = await _service.BuscarPorPetAsync(petId, cancellationToken);

        foreach (var consulta in consultas)
            ComLinks(consulta);

        return Ok(ComoColecao(consultas, nameof(BuscarPorPet), Nome, new { petId }));
    }

    /// <summary>
    /// Retornos sugeridos para os próximos N dias (1 a 365) — alimenta o motor de lembretes preventivos.
    /// </summary>
    [HttpGet("retornos-proximos/{diasAFrente:int}")]
    [ProducesResponseType(typeof(ColecaoDto<ConsultaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ColecaoDto<ConsultaResponseDto>>> BuscarRetornosProximos(int diasAFrente, CancellationToken cancellationToken)
    {
        var consultas = await _service.BuscarRetornosProximosAsync(diasAFrente, cancellationToken);

        foreach (var consulta in consultas)
            ComLinks(consulta);

        return Ok(ComoColecao(consultas, nameof(BuscarRetornosProximos), Nome, new { diasAFrente }));
    }

    /// <summary>Agenda uma nova consulta (status inicial: Agendada).</summary>
    [HttpPost]
    [Authorize(Policy = Politicas.Cadastro)]
    [ProducesResponseType(typeof(ConsultaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ConsultaResponseDto>> Criar([FromBody] CriarConsultaDto dto, CancellationToken cancellationToken)
    {
        var criada = ComLinks(await _service.CriarAsync(dto, cancellationToken));
        return CreatedAtAction(nameof(BuscarPorId), new { id = criada.Id }, criada);
    }

    /// <summary>Registra a evolução da consulta: status, diagnóstico, prescrição e retorno (Admin/Veterinário).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.Clinica)]
    [ProducesResponseType(typeof(ConsultaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ConsultaResponseDto>> Atualizar(int id, [FromBody] AtualizarConsultaDto dto, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.AtualizarAsync(id, dto, cancellationToken)));

    /// <summary>Remove uma consulta. Realizadas não podem ser removidas — apenas canceladas (somente Admin).</summary>
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

    private ConsultaResponseDto ComLinks(ConsultaResponseDto consulta)
    {
        var rota = new { id = consulta.Id };
        AdicionarLink(consulta, "self", HttpMethods.Get, nameof(BuscarPorId), Nome, rota);
        AdicionarLink(consulta, "atualizar", HttpMethods.Put, nameof(Atualizar), Nome, rota);
        AdicionarLink(consulta, "remover", HttpMethods.Delete, nameof(Remover), Nome, rota);
        AdicionarLink(consulta, "pet", HttpMethods.Get, nameof(PetsController.BuscarPorId), PetsController.Nome, new { id = consulta.PetId });
        AdicionarLink(consulta, "veterinario", HttpMethods.Get, nameof(VeterinariosController.BuscarPorId), VeterinariosController.Nome, new { id = consulta.VeterinarioId });
        return consulta;
    }
}
