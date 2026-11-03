namespace Clyvo.PetCare.Domain.Common;

/// <summary>
/// Base das entidades relacionais do domínio. Concentra a identidade
/// (chave gerada pelo banco) para que as entidades só exponham comportamento.
/// </summary>
public abstract class EntityBase
{
    /// <summary>Identificador único, atribuído pela camada de persistência.</summary>
    public int Id { get; protected set; }
}
