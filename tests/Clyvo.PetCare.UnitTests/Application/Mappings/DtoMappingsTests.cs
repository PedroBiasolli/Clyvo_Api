using Clyvo.PetCare.Application.DTOs.Consulta;
using Clyvo.PetCare.Application.DTOs.Vacina;
using Clyvo.PetCare.Application.Mappings;
using Clyvo.PetCare.Domain.Queries;
using Clyvo.PetCare.UnitTests.TestHelpers;

namespace Clyvo.PetCare.UnitTests.Application.Mappings;

/// <summary>Conversões entidade ↔ DTO e query string ↔ filtro. Padrão AAA.</summary>
public class DtoMappingsTests
{
    [Fact]
    public void ParaDto_Pet_DeveIncluirIdadeENomeDoTutor()
    {
        // Arrange
        var tutor = EntityBuilder.NovoTutor(nome: "Juliana Ribeiro");
        var pet = EntityBuilder.NovoPet(tutor, dataNascimento: DateTime.Today.AddYears(-4));

        // Act
        var dto = pet.ParaDto();

        // Assert
        Assert.Equal(4, dto.IdadeAnos);
        Assert.Equal("Juliana Ribeiro", dto.TutorNome);
        Assert.Equal("Macho", dto.SexoDescricao);
        Assert.Empty(dto.Links);
    }

    [Fact]
    public void ParaFiltro_ConsultaSemDirecao_DeveOrdenarPorDataDecrescente()
    {
        // Arrange
        var query = new ConsultaQueryDto { PetId = 3 };

        // Act
        var filtro = query.ParaFiltro();

        // Assert
        Assert.Equal(ConsultaOrdenacao.DataHora, filtro.OrdenarPor);
        Assert.Equal(DirecaoOrdenacao.Desc, filtro.Direcao);
        Assert.Equal(3, filtro.PetId);
    }

    [Fact]
    public void ParaFiltro_VacinaComOrdenacaoInformada_DeveRespeitarEscolha()
    {
        // Arrange
        var query = new VacinaQueryDto { OrdenarPor = VacinaOrdenacao.Nome, Direcao = DirecaoOrdenacao.Asc, Pagina = 3, TamanhoPagina = 5 };

        // Act
        var filtro = query.ParaFiltro();

        // Assert
        Assert.Equal(VacinaOrdenacao.Nome, filtro.OrdenarPor);
        Assert.Equal(DirecaoOrdenacao.Asc, filtro.Direcao);
        Assert.Equal(10, filtro.Deslocamento);
    }

    [Fact]
    public void ParaDto_Consulta_DeveExporDescricoesDeTipoEStatus()
    {
        // Arrange
        var consulta = EntityBuilder.NovaConsulta();

        // Act
        var dto = consulta.ParaDto();

        // Assert
        Assert.Equal("Preventiva", dto.TipoDescricao);
        Assert.Equal("Agendada", dto.StatusDescricao);
        Assert.Equal("Thor", dto.PetNome);
    }
}
