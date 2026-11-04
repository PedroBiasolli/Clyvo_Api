using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Queries;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Infrastructure.Persistence.Repositories;

internal sealed class TutorRepository : RepositoryBase<Tutor>, ITutorRepository
{
    public TutorRepository(AppDbContext contexto) : base(contexto) { }

    protected override IQueryable<Tutor> ComRelacionamentos(IQueryable<Tutor> consulta)
        => consulta.Include(tutor => tutor.Pets);

    public Task<ResultadoPaginado<Tutor>> ListarAsync(TutorFiltro filtro, CancellationToken cancellationToken = default)
    {
        var consulta = ConsultaSomenteLeitura();

        if (TermoDeBusca(filtro.Nome) is { } nome)
            consulta = consulta.Where(t => t.Nome.ToUpper().Contains(nome));

        if (TermoDeBusca(filtro.Cidade) is { } cidade)
            consulta = consulta.Where(t => t.Cidade != null && t.Cidade.ToUpper() == cidade);

        var ordenada = filtro.OrdenarPor switch
        {
            TutorOrdenacao.Cidade => Ordenar(consulta, t => t.Cidade, filtro.Direcao),
            TutorOrdenacao.DataCadastro => Ordenar(consulta, t => t.DataCadastro, filtro.Direcao),
            _ => Ordenar(consulta, t => t.Nome, filtro.Direcao)
        };

        return PaginarAsync(ordenada, filtro, cancellationToken);
    }

    public Task<Tutor?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => ConsultaSomenteLeitura().FirstOrDefaultAsync(t => t.Cpf == cpf, cancellationToken);

    public Task<bool> ExisteCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => Conjunto.AnyAsync(t => t.Cpf == cpf, cancellationToken);

    public Task<bool> PossuiPetsAsync(int tutorId, CancellationToken cancellationToken = default)
        => Contexto.Pets.AnyAsync(p => p.TutorId == tutorId, cancellationToken);
}
