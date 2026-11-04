using Clyvo.PetCare.Api.Hateoas;
using Clyvo.PetCare.Api.Security;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Pet;
using Clyvo.PetCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.Controllers;

/// <summary>Gestão de pets (foco em cachorros nesta versão).</summary>
[Route("api/v1/pets")]
public sealed class PetsController : ApiControllerBase
{
    public const string Nome = "Pets";

    private readonly IPetService _service;

    public PetsController(IPetService service, ILinkService links) : base(links) => _service = service;

    /// <summary>Lista pets com filtros (nome, raça, porte, tutor), ordenação e paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<PetResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<PetResponseDto>>> Listar(
        [FromQuery] PetQueryDto query, CancellationToken cancellationToken)
    {
        var pagina = await _service.ListarAsync(query, cancellationToken);

        foreach (var pet in pagina.Itens)
            ComLinks(pet);

        return Ok(ComLinksDePaginacao(pagina, nameof(Listar), Nome, query));
    }

    /// <summary>Busca um pet pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetResponseDto>> BuscarPorId(int id, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.BuscarPorIdAsync(id, cancellationToken)));

    /// <summary>Lista todos os pets de um tutor.</summary>
    [HttpGet("tutor/{tutorId:int}")]
    [ProducesResponseType(typeof(ColecaoDto<PetResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ColecaoDto<PetResponseDto>>> BuscarPorTutor(int tutorId, CancellationToken cancellationToken)
    {
        var pets = await _service.BuscarPorTutorAsync(tutorId, cancellationToken);

        foreach (var pet in pets)
            ComLinks(pet);

        var colecao = ComoColecao(pets, nameof(BuscarPorTutor), Nome, new { tutorId });
        AdicionarLink(colecao, "tutor", HttpMethods.Get, nameof(TutoresController.BuscarPorId), TutoresController.Nome, new { id = tutorId });
        return Ok(colecao);
    }

    /// <summary>Busca um pet pelo número do microchip.</summary>
    [HttpGet("microchip/{microchip}")]
    [ProducesResponseType(typeof(PetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetResponseDto>> BuscarPorMicrochip(string microchip, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.BuscarPorMicrochipAsync(microchip, cancellationToken)));

    /// <summary>Cadastra um pet vinculado a um tutor existente.</summary>
    [HttpPost]
    [Authorize(Policy = Politicas.Cadastro)]
    [ProducesResponseType(typeof(PetResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PetResponseDto>> Criar([FromBody] CriarPetDto dto, CancellationToken cancellationToken)
    {
        var criado = ComLinks(await _service.CriarAsync(dto, cancellationToken));
        return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza dados clínicos e cadastrais de um pet.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.Cadastro)]
    [ProducesResponseType(typeof(PetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PetResponseDto>> Atualizar(int id, [FromBody] AtualizarPetDto dto, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.AtualizarAsync(id, dto, cancellationToken)));

    /// <summary>Remove um pet sem histórico clínico (somente Admin).</summary>
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

    private PetResponseDto ComLinks(PetResponseDto pet)
    {
        var rota = new { id = pet.Id };
        AdicionarLink(pet, "self", HttpMethods.Get, nameof(BuscarPorId), Nome, rota);
        AdicionarLink(pet, "atualizar", HttpMethods.Put, nameof(Atualizar), Nome, rota);
        AdicionarLink(pet, "remover", HttpMethods.Delete, nameof(Remover), Nome, rota);
        AdicionarLink(pet, "tutor", HttpMethods.Get, nameof(TutoresController.BuscarPorId), TutoresController.Nome, new { id = pet.TutorId });
        AdicionarLink(pet, "consultas", HttpMethods.Get, nameof(ConsultasController.BuscarPorPet), ConsultasController.Nome, new { petId = pet.Id });
        AdicionarLink(pet, "vacinas", HttpMethods.Get, nameof(VacinasController.BuscarPorPet), VacinasController.Nome, new { petId = pet.Id });
        return pet;
    }
}
