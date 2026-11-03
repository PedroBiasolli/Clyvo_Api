using System.Linq.Expressions;
using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Queries;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação base do padrão Repository com EF Core. As leituras para
/// alteração voltam rastreadas; as listagens usam <c>AsNoTracking</c>.
/// </summary>
internal abstract class RepositoryBase<TEntidade> : IRepository<TEntidade> where TEntidade : EntityBase
{
    protected RepositoryBase(AppDbContext contexto)
    {
        Contexto = contexto;
        Conjunto = contexto.Set<TEntidade>();
    }

    protected AppDbContext Contexto { get; }
    protected DbSet<TEntidade> Conjunto { get; }

    public Task<TEntidade?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        => ComRelacionamentos(Conjunto).FirstOrDefaultAsync(entidade => entidade.Id == id, cancellationToken);

    public Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default)
        => Conjunto.AnyAsync(entidade => entidade.Id == id, cancellationToken);

    public async Task AdicionarAsync(TEntidade entidade, CancellationToken cancellationToken = default)
        => await Conjunto.AddAsync(entidade, cancellationToken);

    public void Atualizar(TEntidade entidade)
    {
        // Entidades obtidas pelo repositório já estão rastreadas: o EF detecta
        // as mudanças sozinho. Só anexamos as que chegaram desconectadas.
        if (Contexto.Entry(entidade).State == EntityState.Detached)
            Conjunto.Update(entidade);
    }

    public void Remover(TEntidade entidade) => Conjunto.Remove(entidade);

    /// <summary>Includes usados nas leituras de um único registro (sobrescrito por agregado).</summary>
    protected virtual IQueryable<TEntidade> ComRelacionamentos(IQueryable<TEntidade> consulta) => consulta;

    /// <summary>Base das listagens: somente leitura e com os mesmos relacionamentos.</summary>
    protected IQueryable<TEntidade> ConsultaSomenteLeitura() => ComRelacionamentos(Conjunto.AsNoTracking());

    protected static async Task<ResultadoPaginado<TEntidade>> PaginarAsync(
        IOrderedQueryable<TEntidade> consulta, FiltroPaginado filtro, CancellationToken cancellationToken)
    {
        var total = await consulta.CountAsync(cancellationToken);

        var itens = await consulta
            .ThenBy(entidade => entidade.Id) // desempate estável entre páginas
            .Skip(filtro.Deslocamento)
            .Take(filtro.TamanhoPagina)
            .ToListAsync(cancellationToken);

        return new ResultadoPaginado<TEntidade>(itens, total);
    }

    protected static IOrderedQueryable<TEntidade> Ordenar<TChave>(
        IQueryable<TEntidade> consulta, Expression<Func<TEntidade, TChave>> chave, DirecaoOrdenacao direcao)
        => direcao == DirecaoOrdenacao.Desc ? consulta.OrderByDescending(chave) : consulta.OrderBy(chave);

    /// <summary>Normaliza termos de busca para comparação sem diferenciar maiúsculas (UPPER no SQL).</summary>
    protected static string? TermoDeBusca(string? termo)
        => string.IsNullOrWhiteSpace(termo) ? null : termo.Trim().ToUpper();
}
