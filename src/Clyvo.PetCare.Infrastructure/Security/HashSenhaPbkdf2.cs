using Clyvo.PetCare.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Clyvo.PetCare.Infrastructure.Security;

/// <summary>
/// Hash de senha com o algoritmo do ASP.NET Core Identity
/// (PBKDF2 + HMAC-SHA512, 100 mil iterações, salt aleatório de 128 bits).
/// </summary>
internal sealed class HashSenhaPbkdf2 : IHashSenha
{
    private static readonly object UsuarioIrrelevante = new();
    private readonly PasswordHasher<object> _hasher = new();

    public string Gerar(string senha) => _hasher.HashPassword(UsuarioIrrelevante, senha);

    public bool Verificar(string hash, string senhaInformada)
    {
        try
        {
            return _hasher.VerifyHashedPassword(UsuarioIrrelevante, hash, senhaInformada)
                   != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            // Hash corrompido/inválido no banco: trata como senha incorreta.
            return false;
        }
    }
}
