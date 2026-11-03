using System.ComponentModel.DataAnnotations;
using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.Application.DTOs.Consulta;

/// <summary>Evolução da consulta: status, diagnóstico, prescrição e retorno sugerido.</summary>
public sealed class AtualizarConsultaDto
{
    [Required]
    public DateTime DataHora { get; set; }

    [Required]
    public TipoConsulta Tipo { get; set; }

    [Required]
    public StatusConsulta Status { get; set; }

    [StringLength(1000)]
    public string? MotivoConsulta { get; set; }

    [StringLength(2000)]
    public string? Diagnostico { get; set; }

    [StringLength(2000)]
    public string? Prescricao { get; set; }

    [StringLength(2000)]
    public string? Observacoes { get; set; }

    public DateTime? ProximoRetorno { get; set; }

    public decimal? ValorCobrado { get; set; }
}
