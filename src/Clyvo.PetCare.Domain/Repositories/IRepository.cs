using Clyvo.PetCare.Domain.Common;

namespace Clyvo.PetCare.Domain.Repositories;

/// <summary>
/// Contrato genérico do padrão Repository para as entidades relacionais.
/// A persistência efetiva acontece no <see cref="IUnitOfWork"/>.
/// </summary>
public interface IRepository<TEntidade> where TEntidade : EntityBase
{
    /// <summary>Busca a entidade rastreada (pronta para alteração) pelo identificador.</summary>
    Task<TEntidade?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default);

    Task AdicionarAsync(TEntidade entidade, CancellationToken cancellationToken = default);

    void Atualizar(TEntidade entidade);

    void Remover(TEntidade entidade);
}
