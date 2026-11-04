namespace Clyvo.PetCare.Domain.Common;

/// <summary>
/// Resultado de uma consulta paginada: a fatia de itens solicitada e o total
/// de registros que atendem ao filtro (para cálculo de páginas).
/// </summary>
public sealed record ResultadoPaginado<T>(IReadOnlyList<T> Itens, int TotalRegistros);
