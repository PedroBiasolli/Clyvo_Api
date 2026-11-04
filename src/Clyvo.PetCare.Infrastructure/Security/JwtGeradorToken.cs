using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Clyvo.PetCare.Application.Abstractions;
using Clyvo.PetCare.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clyvo.PetCare.Infrastructure.Security;

/// <summary>Emite JWT assinados com HMAC-SHA256 contendo id, e-mail, nome e perfil do usuário.</summary>
internal sealed class JwtGeradorToken : IGeradorToken
{
    private readonly JwtSettings _settings;

    public JwtGeradorToken(IOptions<JwtSettings> options) => _settings = options.Value;

    public TokenGerado Gerar(Usuario usuario)
    {
        var agora = DateTime.UtcNow;
        var expiraEm = agora.AddMinutes(_settings.ExpiracaoMinutos);

        var claims = new[]
        {
            new Claim(ClaimsClyvo.Id, usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimsClyvo.Email, usuario.Email),
            new Claim(ClaimsClyvo.Nome, usuario.Nome),
            new Claim(ClaimsClyvo.Perfil, usuario.Perfil.ToString()),
            new Claim(ClaimsClyvo.TokenId, Guid.NewGuid().ToString("N"))
        };

        var credenciais = new SigningCredentials(ChaveJwt.Criar(_settings.ChaveSecreta), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: agora,
            expires: expiraEm,
            signingCredentials: credenciais);

        return new TokenGerado(new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
