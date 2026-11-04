namespace Clyvo.PetCare.Application.DTOs.Common;

/// <summary>
/// Base de toda representação devolvida pela API: carrega os links HATEOAS
/// que a camada de apresentação preenche conforme o contexto da requisição.
/// </summary>
public abstract class RecursoDto
{
    /// <summary>Links navegáveis a partir deste recurso.</summary>
    public IList<LinkDto> Links { get; set; } = new List<LinkDto>();
}
