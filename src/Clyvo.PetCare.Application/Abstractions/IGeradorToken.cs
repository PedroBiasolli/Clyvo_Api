using Clyvo.PetCare.Domain.Entities;

namespace Clyvo.PetCare.Application.Abstractions;

/// <summary>Token de acesso emitido para um usuário autenticado.</summary>
public sealed record TokenGerado(string Token, DateTime ExpiraEmUtc);

/// <summary>Emissão de tokens de acesso (JWT na infraestrutura).</summary>
public interface IGeradorToken
{
    TokenGerado Gerar(Usuario usuario);
}
