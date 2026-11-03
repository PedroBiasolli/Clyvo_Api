using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Queries;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Infrastructure.Persistence.Repositories;

internal sealed class ConsultaRepository : RepositoryBase<Consulta>, IConsultaRepository
{
    public ConsultaRepository(AppDbContext contexto) : base(contexto) { }

    protected override IQueryable<Consulta> ComRelacionamentos(IQueryable<Consulta> consulta)
        => consulta.Include(c => c.Pet).Include(c => c.Veterinario);

    public Task<ResultadoPaginado<Consulta>> ListarAsync(ConsultaFiltro filtro, CancellationToken cancellationToken = default)
    {
        var consulta = ConsultaSomenteLeitura();

        if (filtro.Tipo.HasValue)
            consulta = consulta.Where(c => c.Tipo == filtro.Tipo.Value);

        if (filtro.Status.HasValue)
            consulta = consulta.Where(c => c.Status == filtro.Status.Value);

        if (filtro.PetId.HasValue)
            consulta = consulta.Where(c => c.PetId == filtro.PetId.Value);

        if (filtro.VeterinarioId.HasValue)
            consulta = consulta.Where(c => c.VeterinarioId == filtro.VeterinarioId.Value);

        if (filtro.DataInicio.HasValue)
            consulta = consulta.Where(c => c.DataHora >= filtro.DataInicio.Value);

        if (filtro.DataFim.HasValue)
            consulta = consulta.Where(c => c.DataHora <= filtro.DataFim.Value);

        var ordenada = filtro.OrdenarPor switch
        {
            ConsultaOrdenacao.Tipo => Ordenar(consulta, c => c.Tipo, filtro.Direcao),
            ConsultaOrdenacao.Status => Ordenar(consulta, c => c.Status, filtro.Direcao),
            ConsultaOrdenacao.ProximoRetorno => Ordenar(consulta, c => c.ProximoRetorno, filtro.Direcao),
            _ => Ordenar(consulta, c => c.DataHora, filtro.Direcao)
        };

        return PaginarAsync(ordenada, filtro, cancellationToken);
    }

    public async Task<IReadOnlyList<Consulta>> ListarPorPetAsync(int petId, CancellationToken cancellationToken = default)
        => await ConsultaSomenteLeitura()
            .Where(c => c.PetId == petId)
            .OrderByDescending(c => c.DataHora)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Consulta>> ListarRetornosEntreAsync(DateTime inicio, DateTime fim, CancellationToken cancellationToken = default)
    {
        // "fim" é inclusivo por data: retornos ao longo de todo o último dia entram.
        var limiteExclusivo = fim.Date.AddDays(1);

        return await ConsultaSomenteLeitura()
            .Where(c => c.ProximoRetorno != null
                        && c.ProximoRetorno >= inicio
                        && c.ProximoRetorno < limiteExclusivo)
            .OrderBy(c => c.ProximoRetorno)
            .ToListAsync(cancellationToken);
    }
}
