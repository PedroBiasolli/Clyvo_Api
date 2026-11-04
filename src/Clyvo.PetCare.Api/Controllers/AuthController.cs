using Clyvo.PetCare.Api.Hateoas;
using Clyvo.PetCare.Api.Security;
using Clyvo.PetCare.Application.DTOs.Auth;
using Clyvo.PetCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.Controllers;

/// <summary>Autenticação JWT e gestão de usuários da equipe.</summary>
[Route("api/v1/auth")]
public sealed class AuthController : ApiControllerBase
{
    public const string Nome = "Auth";

    private readonly IAuthService _service;

    public AuthController(IAuthService service, ILinkService links) : base(links) => _service = service;

    /// <summary>Autentica com e-mail e senha e devolve um token JWT (Bearer).</summary>
    /// <remarks>Usuário inicial (seed): <c>admin@clyvo.com.br</c> / senha definida em <c>SeedAdmin:Senha</c>.</remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var token = await _service.LoginAsync(dto, cancellationToken);
        AdicionarLink(token, "usuario-atual", HttpMethods.Get, nameof(UsuarioLogado), Nome);
        return Ok(token);
    }

    /// <summary>Dados do usuário autenticado (a partir do token).</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioDto>> UsuarioLogado(CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.BuscarUsuarioAtualAsync(cancellationToken)));

    /// <summary>Busca um usuário pelo identificador (somente Admin).</summary>
    [HttpGet("usuarios/{id:int}")]
    [Authorize(Policy = Politicas.Administracao)]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> BuscarUsuario(int id, CancellationToken cancellationToken)
        => Ok(ComLinks(await _service.BuscarUsuarioPorIdAsync(id, cancellationToken)));

    /// <summary>Cria um usuário da equipe com perfil Admin, Veterinario ou Atendente (somente Admin).</summary>
    [HttpPost("usuarios")]
    [Authorize(Policy = Politicas.Administracao)]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioDto>> CriarUsuario([FromBody] CriarUsuarioDto dto, CancellationToken cancellationToken)
    {
        var criado = ComLinks(await _service.CriarUsuarioAsync(dto, cancellationToken));
        return CreatedAtAction(nameof(BuscarUsuario), new { id = criado.Id }, criado);
    }

    private UsuarioDto ComLinks(UsuarioDto usuario)
    {
        AdicionarLink(usuario, "self", HttpMethods.Get, nameof(BuscarUsuario), Nome, new { id = usuario.Id });
        return usuario;
    }
}
