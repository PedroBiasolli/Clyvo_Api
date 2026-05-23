namespace Clyvo.PetCare.Api.Application.DTOs.Common;

/// <summary>
/// Envelope padronizado para respostas paginadas.
/// </summary>
public class PagedResultDto<T>
{
    public IEnumerable<T> Itens { get; set; } = Enumerable.Empty<T>();
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(TotalRegistros / (double)TamanhoPagina);
}
