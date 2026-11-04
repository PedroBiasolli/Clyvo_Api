using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Infrastructure.Persistence.Repositories;

internal sealed class UsuarioRepository : RepositoryBase<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext contexto) : base(contexto) { }

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
        => Conjunto.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        => Conjunto.AnyAsync(u => u.Email == email, cancellationToken);
}
