using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Clyvo.PetCare.Infrastructure.Security;

/// <summary>Chave simétrica compartilhada entre a emissão (aqui) e a validação (API) do JWT.</summary>
public static class ChaveJwt
{
    public static SymmetricSecurityKey Criar(string chaveSecreta)
        => new(Encoding.UTF8.GetBytes(chaveSecreta));
}
