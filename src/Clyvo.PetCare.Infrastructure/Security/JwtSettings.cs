namespace Clyvo.PetCare.Infrastructure.Security;

/// <summary>Parâmetros de emissão/validação do JWT (seção <c>Jwt</c> do appsettings).</summary>
public sealed class JwtSettings
{
    public const string Secao = "Jwt";

    /// <summary>HS256 exige chave de no mínimo 256 bits.</summary>
    public const int TamanhoMinimoChaveBytes = 32;

    public string Issuer { get; set; } = "Clyvo.PetCare.Api";
    public string Audience { get; set; } = "Clyvo.PetCare.Clientes";
    public string ChaveSecreta { get; set; } = string.Empty;
    public int ExpiracaoMinutos { get; set; } = 120;
}
