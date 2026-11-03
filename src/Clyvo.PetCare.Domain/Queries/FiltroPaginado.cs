namespace Clyvo.PetCare.Domain.Queries;

/// <summary>Direção de ordenação aceita pelas listagens.</summary>
public enum DirecaoOrdenacao
{
    Asc = 1,
    Desc = 2
}

/// <summary>
/// Base dos filtros de listagem: paginação + direção de ordenação.
/// Os repositórios recebem estes objetos (Query Objects) e traduzem para LINQ.
/// </summary>
public abstract class FiltroPaginado
{
    public const int TamanhoPaginaPadrao = 20;
    public const int TamanhoPaginaMaximo = 100;

    private readonly int _pagina = 1;
    private readonly int _tamanhoPagina = TamanhoPaginaPadrao;

    public int Pagina
    {
        get => _pagina;
        init => _pagina = value < 1 ? 1 : value;
    }

    public int TamanhoPagina
    {
        get => _tamanhoPagina;
        init => _tamanhoPagina = value is < 1 or > TamanhoPaginaMaximo ? TamanhoPaginaPadrao : value;
    }

    public DirecaoOrdenacao Direcao { get; init; } = DirecaoOrdenacao.Asc;

    /// <summary>Quantidade de registros a pular (SKIP/OFFSET).</summary>
    public int Deslocamento => (Pagina - 1) * TamanhoPagina;
}
