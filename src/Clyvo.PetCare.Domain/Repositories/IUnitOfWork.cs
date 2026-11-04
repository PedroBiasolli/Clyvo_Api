namespace Clyvo.PetCare.Domain.Repositories;

/// <summary>
/// Unidade de trabalho: confirma, numa única transação, todas as alterações
/// feitas pelos repositórios durante a requisição.
/// </summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
