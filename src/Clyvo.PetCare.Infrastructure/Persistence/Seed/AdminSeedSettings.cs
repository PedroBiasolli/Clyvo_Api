namespace Clyvo.PetCare.Infrastructure.Persistence.Seed;

/// <summary>Usuário administrador criado automaticamente quando ainda não existe (seção <c>SeedAdmin</c>).</summary>
public sealed class AdminSeedSettings
{
    public const string Secao = "SeedAdmin";

    public bool Habilitado { get; set; } = true;
    public string Nome { get; set; } = "Administrador Clyvo";
    public string Email { get; set; } = "admin@clyvo.com.br";
    public string Senha { get; set; } = string.Empty;
}
