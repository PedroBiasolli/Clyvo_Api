using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Queries;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Infrastructure.Persistence.Repositories;

internal sealed class VacinaRepository : RepositoryBase<Vacina>, IVacinaRepository
{
    public VacinaRepository(AppDbContext contexto) : base(contexto) { }

    protected override IQueryable<Vacina> ComRelacionamentos(IQueryable<Vacina> consulta)
        => consulta.Include(v => v.Pet).Include(v => v.Veterinario);

    public Task<ResultadoPaginado<Vacina>> ListarAsync(VacinaFiltro filtro, CancellationToken cancellationToken = default)
    {
        var consulta = ConsultaSomenteLeitura();

        if (TermoDeBusca(filtro.Nome) is { } nome)
            consulta = consulta.Where(v => v.Nome.ToUpper().Contains(nome));

        if (filtro.PetId.HasValue)
            consulta = consulta.Where(v => v.PetId == filtro.PetId.Value);

        var ordenada = filtro.OrdenarPor switch
        {
            VacinaOrdenacao.DataProximaDose => Ordenar(consulta, v => v.DataProximaDose, filtro.Direcao),
            VacinaOrdenacao.Nome => Ordenar(consulta, v => v.Nome, filtro.Direcao),
            _ => Ordenar(consulta, v => v.DataAplicacao, filtro.Direcao)
        };

        return PaginarAsync(ordenada, filtro, cancellationToken);
    }

    public async Task<IReadOnlyList<Vacina>> ListarPorPetAsync(int petId, CancellationToken cancellationToken = default)
        => await ConsultaSomenteLeitura()
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DataAplicacao)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Vacina>> ListarAtrasadasAsync(DateTime referencia, CancellationToken cancellationToken = default)
    {
        var hoje = referencia.Date;

        return await ConsultaSomenteLeitura()
            .Where(v => v.DataProximaDose != null && v.DataProximaDose < hoje)
            .OrderBy(v => v.DataProximaDose)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vacina>> ListarProximasDosesEntreAsync(DateTime inicio, DateTime fim, CancellationToken cancellationToken = default)
    {
        var limiteExclusivo = fim.Date.AddDays(1);

        return await ConsultaSomenteLeitura()
            .Where(v => v.DataProximaDose != null
                        && v.DataProximaDose >= inicio
                        && v.DataProximaDose < limiteExclusivo)
            .OrderBy(v => v.DataProximaDose)
            .ToListAsync(cancellationToken);
    }
}
