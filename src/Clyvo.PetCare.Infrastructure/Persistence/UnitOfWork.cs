using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Infrastructure.Persistence;

/// <summary>
/// Unit of Work sobre o <see cref="AppDbContext"/>. Traduz falhas de
/// integridade do banco (unique, FK) em <see cref="ConflitoException"/>,
/// para que a API responda 409 em vez de 500.
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _contexto;

    public UnitOfWork(AppDbContext contexto) => _contexto = contexto;

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflitoException("O registro foi alterado ou removido por outra operação. Recarregue e tente novamente.", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new ConflitoException("A operação viola uma restrição de integridade do banco (registro duplicado ou em uso).", ex);
        }
    }
}
