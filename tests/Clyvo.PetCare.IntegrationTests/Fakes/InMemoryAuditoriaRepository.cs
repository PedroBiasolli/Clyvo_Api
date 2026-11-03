using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Queries;
using Clyvo.PetCare.Domain.Repositories;

namespace Clyvo.PetCare.IntegrationTests.Fakes;

/// <summary>
/// Substituto em memória do repositório MongoDB, para que os testes de
/// integração rodem sem um servidor Mongo e possam inspecionar a auditoria.
/// </summary>
public sealed class InMemoryAuditoriaRepository : IAuditoriaRepository
{
    private readonly List<RegistroAuditoria> _registros = new();
    private readonly object _trava = new();

    public IReadOnlyList<RegistroAuditoria> Registros
    {
        get { lock (_trava) return _registros.ToList(); }
    }

    public void Limpar()
    {
        lock (_trava) _registros.Clear();
    }

    public Task InserirAsync(RegistroAuditoria registro, CancellationToken cancellationToken = default)
    {
        var persistido = RegistroAuditoria.Reconstituir(
            Guid.NewGuid().ToString("N")[..24], registro.Entidade, registro.EntidadeId, registro.Acao,
            registro.Usuario, registro.DataHoraUtc, registro.CorrelationId, registro.DadosJson);

        lock (_trava) _registros.Add(persistido);
        return Task.CompletedTask;
    }

    public Task<ResultadoPaginado<RegistroAuditoria>> ListarAsync(AuditoriaFiltro filtro, CancellationToken cancellationToken = default)
    {
        var consulta = Registros.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filtro.Entidade))
            consulta = consulta.Where(r => r.Entidade.Equals(filtro.Entidade, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(filtro.EntidadeId))
            consulta = consulta.Where(r => r.EntidadeId == filtro.EntidadeId);
        if (filtro.Acao.HasValue)
            consulta = consulta.Where(r => r.Acao == filtro.Acao.Value);
        if (!string.IsNullOrWhiteSpace(filtro.Usuario))
            consulta = consulta.Where(r => r.Usuario.Contains(filtro.Usuario, StringComparison.OrdinalIgnoreCase));

        var ordenada = filtro.Direcao == DirecaoOrdenacao.Asc
            ? consulta.OrderBy(r => r.DataHoraUtc)
            : consulta.OrderByDescending(r => r.DataHoraUtc);

        var lista = ordenada.ToList();
        var pagina = lista.Skip(filtro.Deslocamento).Take(filtro.TamanhoPagina).ToList();
        return Task.FromResult(new ResultadoPaginado<RegistroAuditoria>(pagina, lista.Count));
    }

    public Task<RegistroAuditoria?> ObterPorIdAsync(string id, CancellationToken cancellationToken = default)
        => Task.FromResult(Registros.FirstOrDefault(r => r.Id == id));

    public Task<IReadOnlyList<ResumoAuditoria>> ResumirPorEntidadeEAcaoAsync(DateTime? desde, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ResumoAuditoria> resumo = Registros
            .Where(r => !desde.HasValue || r.DataHoraUtc >= desde.Value)
            .GroupBy(r => (r.Entidade, r.Acao))
            .Select(g => new ResumoAuditoria(g.Key.Entidade, g.Key.Acao, g.LongCount()))
            .OrderBy(r => r.Entidade).ThenBy(r => r.Acao)
            .ToList();

        return Task.FromResult(resumo);
    }
}
