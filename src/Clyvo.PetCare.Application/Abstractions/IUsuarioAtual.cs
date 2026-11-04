namespace Clyvo.PetCare.Application.Abstractions;

/// <summary>
/// Quem está executando a operação. Implementado na camada de API a partir
/// das claims do JWT — a aplicação não conhece HttpContext.
/// </summary>
public interface IUsuarioAtual
{
    /// <summary>E-mail do usuário autenticado, ou "anonimo".</summary>
    string Identificador { get; }

    /// <summary>Correlation ID da requisição em curso (para rastrear a auditoria).</summary>
    string? CorrelationId { get; }
}
