namespace Clyvo.PetCare.Application.DTOs.Common;

/// <summary>
/// Hipermídia (HATEOAS): ação disponível a partir do recurso atual.
/// </summary>
/// <param name="Href">URL absoluta da ação.</param>
/// <param name="Rel">Relação com o recurso (self, atualizar, remover, proxima-pagina...).</param>
/// <param name="Metodo">Verbo HTTP a ser usado (GET, POST, PUT, DELETE).</param>
public sealed record LinkDto(string Href, string Rel, string Metodo);
