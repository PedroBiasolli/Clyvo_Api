using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.Domain.Queries;

public enum TutorOrdenacao { Nome = 1, Cidade = 2, DataCadastro = 3 }

public sealed class TutorFiltro : FiltroPaginado
{
    public string? Nome { get; init; }
    public string? Cidade { get; init; }
    public TutorOrdenacao OrdenarPor { get; init; } = TutorOrdenacao.Nome;
}

public enum PetOrdenacao { Nome = 1, Raca = 2, DataNascimento = 3, PesoKg = 4, DataCadastro = 5 }

public sealed class PetFiltro : FiltroPaginado
{
    public string? Nome { get; init; }
    public string? Raca { get; init; }
    public PorteCachorro? Porte { get; init; }
    public int? TutorId { get; init; }
    public PetOrdenacao OrdenarPor { get; init; } = PetOrdenacao.Nome;
}

public enum VeterinarioOrdenacao { Nome = 1, Especialidade = 2, DataCadastro = 3 }

public sealed class VeterinarioFiltro : FiltroPaginado
{
    public string? Nome { get; init; }
    public string? Especialidade { get; init; }
    public bool? Ativo { get; init; }
    public VeterinarioOrdenacao OrdenarPor { get; init; } = VeterinarioOrdenacao.Nome;
}

public enum ConsultaOrdenacao { DataHora = 1, Tipo = 2, Status = 3, ProximoRetorno = 4 }

public sealed class ConsultaFiltro : FiltroPaginado
{
    public TipoConsulta? Tipo { get; init; }
    public StatusConsulta? Status { get; init; }
    public int? PetId { get; init; }
    public int? VeterinarioId { get; init; }
    public DateTime? DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
    public ConsultaOrdenacao OrdenarPor { get; init; } = ConsultaOrdenacao.DataHora;
}

public enum VacinaOrdenacao { DataAplicacao = 1, DataProximaDose = 2, Nome = 3 }

public sealed class VacinaFiltro : FiltroPaginado
{
    public string? Nome { get; init; }
    public int? PetId { get; init; }
    public VacinaOrdenacao OrdenarPor { get; init; } = VacinaOrdenacao.DataAplicacao;
}

public sealed class AuditoriaFiltro : FiltroPaginado
{
    public string? Entidade { get; init; }
    public string? EntidadeId { get; init; }
    public AcaoAuditoria? Acao { get; init; }
    public string? Usuario { get; init; }
    public DateTime? DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
}

/// <summary>Linha do resumo agregado da auditoria (resultado de aggregation no MongoDB).</summary>
public sealed record ResumoAuditoria(string Entidade, AcaoAuditoria Acao, long Total);
