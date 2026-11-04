namespace Clyvo.PetCare.Application.Abstractions;

/// <summary>
/// Contrato das claims do token de acesso (nomes curtos do padrão JWT).
/// Compartilhado entre quem emite o token (Infraestrutura) e quem o lê (Api),
/// sem que uma camada externa dependa da outra.
/// </summary>
public static class ClaimsClyvo
{
    public const string Id = "sub";
    public const string Email = "email";
    public const string Nome = "name";
    public const string Perfil = "role";
    public const string TokenId = "jti";
}
