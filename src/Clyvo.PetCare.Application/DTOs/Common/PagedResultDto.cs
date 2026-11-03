namespace Clyvo.PetCare.Application.DTOs.Common;

/// <summary>
/// Envelope padronizado para respostas paginadas, com metadados de paginação
/// e links de navegação (primeira, anterior, próxima, última página).
/// </summary>
public class PagedResultDto<T> : RecursoDto
{
    public IReadOnlyList<T> Itens { get; set; } = Array.Empty<T>();
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalRegistros { get; set; }

    public int TotalPaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(TotalRegistros / (double)TamanhoPagina);
    public bool TemPaginaAnterior => Pagina > 1;
    public bool TemProximaPagina => Pagina < TotalPaginas;
}
