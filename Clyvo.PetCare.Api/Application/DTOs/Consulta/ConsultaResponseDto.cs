using Clyvo.PetCare.Api.Domain.Enums;

namespace Clyvo.PetCare.Api.Application.DTOs.Consulta;

public class ConsultaResponseDto
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public TipoConsulta Tipo { get; set; }
    public string TipoDescricao => Tipo.ToString();
    public StatusConsulta Status { get; set; }
    public string StatusDescricao => Status.ToString();
    public string? MotivoConsulta { get; set; }
    public string? Diagnostico { get; set; }
    public string? Prescricao { get; set; }
    public string? Observacoes { get; set; }
    public DateTime? ProximoRetorno { get; set; }
    public decimal? ValorCobrado { get; set; }
    public DateTime DataCriacao { get; set; }
    public int PetId { get; set; }
    public string? PetNome { get; set; }
    public int VeterinarioId { get; set; }
    public string? VeterinarioNome { get; set; }
}
