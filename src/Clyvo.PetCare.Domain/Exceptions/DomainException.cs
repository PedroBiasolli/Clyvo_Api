namespace Clyvo.PetCare.Domain.Exceptions;

/// <summary>
/// Violação de uma regra de negócio ou invariante do domínio.
/// A camada de API traduz esta exceção para HTTP 422 (Unprocessable Entity).
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
