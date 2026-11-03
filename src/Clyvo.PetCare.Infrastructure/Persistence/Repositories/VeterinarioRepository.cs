using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Queries;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Infrastructure.Persistence.Repositories;

internal sealed class VeterinarioRepository : RepositoryBase<Veterinario>, IVeterinarioRepository
{
    public VeterinarioRepository(AppDbContext contexto) : base(contexto) { }

    public Task<ResultadoPaginado<Veterinario>> ListarAsync(VeterinarioFiltro filtro, CancellationToken cancellationToken = default)
    {
        var consulta = ConsultaSomenteLeitura();

        if (TermoDeBusca(filtro.Nome) is { } nome)
            consulta = consulta.Where(v => v.Nome.ToUpper().Contains(nome));

        if (TermoDeBusca(filtro.Especialidade) is { } especialidade)
            consulta = consulta.Where(v => v.Especialidade != null && v.Especialidade.ToUpper().Contains(especialidade));

        if (filtro.Ativo.HasValue)
            consulta = consulta.Where(v => v.Ativo == filtro.Ativo.Value);

        var ordenada = filtro.OrdenarPor switch
        {
            VeterinarioOrdenacao.Especialidade => Ordenar(consulta, v => v.Especialidade, filtro.Direcao),
            VeterinarioOrdenacao.DataCadastro => Ordenar(consulta, v => v.DataCadastro, filtro.Direcao),
            _ => Ordenar(consulta, v => v.Nome, filtro.Direcao)
        };

        return PaginarAsync(ordenada, filtro, cancellationToken);
    }

    public Task<Veterinario?> ObterPorCrmvAsync(string crmv, CancellationToken cancellationToken = default)
        => ConsultaSomenteLeitura().FirstOrDefaultAsync(v => v.Crmv == crmv, cancellationToken);

    public Task<bool> ExisteCrmvAsync(string crmv, CancellationToken cancellationToken = default)
        => Conjunto.AnyAsync(v => v.Crmv == crmv, cancellationToken);

    public Task<bool> PossuiConsultasAsync(int veterinarioId, CancellationToken cancellationToken = default)
        => Contexto.Consultas.AnyAsync(c => c.VeterinarioId == veterinarioId, cancellationToken);
}
