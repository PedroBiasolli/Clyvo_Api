using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Queries;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Infrastructure.Persistence.Repositories;

internal sealed class PetRepository : RepositoryBase<Pet>, IPetRepository
{
    public PetRepository(AppDbContext contexto) : base(contexto) { }

    protected override IQueryable<Pet> ComRelacionamentos(IQueryable<Pet> consulta)
        => consulta.Include(pet => pet.Tutor);

    public Task<ResultadoPaginado<Pet>> ListarAsync(PetFiltro filtro, CancellationToken cancellationToken = default)
    {
        var consulta = ConsultaSomenteLeitura();

        if (TermoDeBusca(filtro.Nome) is { } nome)
            consulta = consulta.Where(p => p.Nome.ToUpper().Contains(nome));

        if (TermoDeBusca(filtro.Raca) is { } raca)
            consulta = consulta.Where(p => p.Raca.ToUpper().Contains(raca));

        if (filtro.Porte.HasValue)
            consulta = consulta.Where(p => p.Porte == filtro.Porte.Value);

        if (filtro.TutorId.HasValue)
            consulta = consulta.Where(p => p.TutorId == filtro.TutorId.Value);

        var ordenada = filtro.OrdenarPor switch
        {
            PetOrdenacao.Raca => Ordenar(consulta, p => p.Raca, filtro.Direcao),
            PetOrdenacao.DataNascimento => Ordenar(consulta, p => p.DataNascimento, filtro.Direcao),
            PetOrdenacao.PesoKg => Ordenar(consulta, p => p.PesoKg, filtro.Direcao),
            PetOrdenacao.DataCadastro => Ordenar(consulta, p => p.DataCadastro, filtro.Direcao),
            _ => Ordenar(consulta, p => p.Nome, filtro.Direcao)
        };

        return PaginarAsync(ordenada, filtro, cancellationToken);
    }

    public async Task<IReadOnlyList<Pet>> ListarPorTutorAsync(int tutorId, CancellationToken cancellationToken = default)
        => await ConsultaSomenteLeitura()
            .Where(p => p.TutorId == tutorId)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);

    public Task<Pet?> ObterPorMicrochipAsync(string microchip, CancellationToken cancellationToken = default)
        => ConsultaSomenteLeitura().FirstOrDefaultAsync(p => p.Microchip == microchip, cancellationToken);

    public Task<bool> ExisteMicrochipAsync(string microchip, int? ignorarPetId = null, CancellationToken cancellationToken = default)
    {
        var consulta = Conjunto.Where(p => p.Microchip == microchip);

        if (ignorarPetId.HasValue)
            consulta = consulta.Where(p => p.Id != ignorarPetId.Value);

        return consulta.AnyAsync(cancellationToken);
    }

    public async Task<bool> PossuiHistoricoClinicoAsync(int petId, CancellationToken cancellationToken = default)
        => await Contexto.Consultas.AnyAsync(c => c.PetId == petId, cancellationToken)
           || await Contexto.Vacinas.AnyAsync(v => v.PetId == petId, cancellationToken);
}
