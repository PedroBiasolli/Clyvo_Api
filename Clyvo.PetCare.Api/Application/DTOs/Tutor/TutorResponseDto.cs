namespace Clyvo.PetCare.Api.Application.DTOs.Tutor;

public class TutorResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public DateTime DataCadastro { get; set; }
    public bool AceitaNotificacoes { get; set; }
    public int QuantidadePets { get; set; }
}
