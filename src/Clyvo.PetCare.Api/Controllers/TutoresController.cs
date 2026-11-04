using Clyvo.PetCare.Api.Hateoas;
using Clyvo.PetCare.Api.Security;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Tutor;
using Clyvo.PetCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.Controllers;

/// <summary>Gestão de tutores (responsáveis legais pelos pets).</summary>
[Route("api/v1/tutores")]
public sealed class TutoresController : ApiControllerBase
{
    public const string Nome = "Tutores";

    private readonly ITutorService _service;

    public TutoresController(ITutorService service, ILinkService links) : base(links) => _service = service;

    /// <summary>Lista tutores com filtros, ordenação e paginação.</summary>
    /// <remarks>Exemplo: <c>GET /api/v1/tutores?cidade=São Paulo&amp;ordenarPor=DataCadastro&amp;direcao=Desc&amp;pagina=1&amp;tamanhoPagina=10</c></remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TutorResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<TutorResponseDto>>> Listar(
        [FromQuery] TutorQueryDto query, CancellationToken cancellationToken)
    {
        var pagina = await _service.ListarAsync(query, cancellationToken);

        foreach (var tutor in pagina.Itens)
            ComLinks(tutor);

        return Ok(ComLinksDePaginacao(pagina, nameof(Listar), Nome, query));
    }

    /// <summary>Busca um tutor pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TutorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TutorResponseDto>> BuscarPorId(int id, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.BuscarPorIdAsync(id, cancellationToken)));

    /// <summary>Busca um tutor pelo CPF (com ou sem pontuação).</summary>
    [HttpGet("cpf/{cpf}")]
    [ProducesResponseType(typeof(TutorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TutorResponseDto>> BuscarPorCpf(string cpf, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.BuscarPorCpfAsync(cpf, cancellationToken)));

    /// <summary>Cadastra um novo tutor.</summary>
    [HttpPost]
    [Authorize(Policy = Politicas.Cadastro)]
    [ProducesResponseType(typeof(TutorResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TutorResponseDto>> Criar([FromBody] CriarTutorDto dto, CancellationToken cancellationToken)
    {
        var criado = ComLinks(await _service.CriarAsync(dto, cancellationToken));
        return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza os dados de um tutor (o CPF é imutável).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.Cadastro)]
    [ProducesResponseType(typeof(TutorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TutorResponseDto>> Atualizar(int id, [FromBody] AtualizarTutorDto dto, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.AtualizarAsync(id, dto, cancellationToken)));

    /// <summary>Remove um tutor sem pets cadastrados (somente Admin).</summary>
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

    private TutorResponseDto ComLinks(TutorResponseDto tutor)
    {
        var rota = new { id = tutor.Id };
        AdicionarLink(tutor, "self", HttpMethods.Get, nameof(BuscarPorId), Nome, rota);
        AdicionarLink(tutor, "atualizar", HttpMethods.Put, nameof(Atualizar), Nome, rota);
        AdicionarLink(tutor, "remover", HttpMethods.Delete, nameof(Remover), Nome, rota);
        AdicionarLink(tutor, "pets", HttpMethods.Get, nameof(PetsController.BuscarPorTutor), PetsController.Nome, new { tutorId = tutor.Id });
        return tutor;
    }
}
