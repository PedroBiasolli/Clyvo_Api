namespace Clyvo.PetCare.Infrastructure.Security;

/// <summary>
/// Nomes curtos (padrão JWT) das claims emitidas. A validação desliga o
/// mapeamento automático de claims, então emissor e validador usam estes nomes.
/// </summary>
public static class ClaimsClyvo
{
    public const string Id = "sub";
    public const string Email = "email";
    public const string Nome = "name";
    public const string Perfil = "role";
    public const string TokenId = "jti";
}
