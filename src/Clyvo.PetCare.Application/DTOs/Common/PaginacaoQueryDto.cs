using System.ComponentModel.DataAnnotations;
using Clyvo.PetCare.Domain.Queries;

namespace Clyvo.PetCare.Application.DTOs.Common;

/// <summary>
/// Parâmetros comuns de listagem recebidos pela query string.
/// </summary>
public abstract class PaginacaoQueryDto
{
    /// <summary>Número da página (começa em 1).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior ou igual a 1.")]
    public int Pagina { get; set; } = 1;

    /// <summary>Quantidade de itens por página (1 a 100).</summary>
    [Range(1, FiltroPaginado.TamanhoPaginaMaximo, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = FiltroPaginado.TamanhoPaginaPadrao;

    /// <summary>Direção da ordenação (Asc ou Desc). Cada recurso tem seu padrão.</summary>
    public DirecaoOrdenacao? Direcao { get; set; }
}
