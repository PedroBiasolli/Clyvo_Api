namespace Clyvo.PetCare.Application.Abstractions;

/// <summary>Geração e verificação de hash de senha (implementado com PBKDF2 na infraestrutura).</summary>
public interface IHashSenha
{
    string Gerar(string senha);
    bool Verificar(string hash, string senhaInformada);
}
