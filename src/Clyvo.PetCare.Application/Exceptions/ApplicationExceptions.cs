namespace Clyvo.PetCare.Application.Exceptions;

/// <summary>O recurso solicitado não existe. Traduzida para HTTP 404.</summary>
public sealed class RecursoNaoEncontradoException : Exception
{
    public RecursoNaoEncontradoException(string recurso, object chave)
        : base($"{recurso} '{chave}' não encontrado(a).")
    {
        Recurso = recurso;
        Chave = chave;
    }

    public string Recurso { get; }
    public object Chave { get; }
}

/// <summary>Conflito com o estado atual (ex.: CPF, CRMV ou e-mail já cadastrados). HTTP 409.</summary>
public sealed class ConflitoException : Exception
{
    public ConflitoException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}

/// <summary>
/// Regra de negócio verificada na camada de aplicação (depende de consulta a
/// repositórios, ex.: "tutor com pets não pode ser removido"). HTTP 422.
/// </summary>
public sealed class RegraDeNegocioException : Exception
{
    public RegraDeNegocioException(string message) : base(message) { }
}

/// <summary>E-mail ou senha inválidos, ou usuário inativo. HTTP 401.</summary>
public sealed class CredenciaisInvalidasException : Exception
{
    public CredenciaisInvalidasException()
        : base("E-mail ou senha inválidos.") { }
}
