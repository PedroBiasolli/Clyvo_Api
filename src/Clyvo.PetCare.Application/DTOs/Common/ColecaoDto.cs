namespace Clyvo.PetCare.Application.DTOs.Common;

/// <summary>
/// Coleção não paginada (ex.: histórico de um pet), com link de navegação própria.
/// </summary>
public class ColecaoDto<T> : RecursoDto
{
    public IReadOnlyList<T> Itens { get; set; } = Array.Empty<T>();
    public int Total => Itens.Count;
}
