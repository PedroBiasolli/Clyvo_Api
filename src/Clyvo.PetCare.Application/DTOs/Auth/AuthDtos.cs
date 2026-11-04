using System.ComponentModel.DataAnnotations;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.Application.DTOs.Auth;

/// <summary>Credenciais para obter um token JWT.</summary>
public sealed class LoginDto
{
    [Required(ErrorMessage = "E-mail é obrigatório."), EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória."), StringLength(100)]
    public string Senha { get; set; } = string.Empty;
}

/// <summary>Cadastro de um novo usuário da equipe (somente administradores).</summary>
public sealed class CriarUsuarioDto
{
    [Required, StringLength(120, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Mínimo de 8 caracteres, com letra maiúscula, minúscula e número.</summary>
    [Required, StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "A senha deve conter ao menos uma letra maiúscula, uma minúscula e um número.")]
    public string Senha { get; set; } = string.Empty;

    [Required]
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Atendente;
}

/// <summary>Dados públicos de um usuário (nunca inclui a senha).</summary>
public sealed class UsuarioDto : RecursoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
}

/// <summary>Resposta do login: token Bearer e seus metadados.</summary>
public sealed class TokenDto : RecursoDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiraEmUtc { get; set; }
    public UsuarioDto Usuario { get; set; } = new();
}
