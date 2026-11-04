using Clyvo.PetCare.Api.Hateoas;
using Clyvo.PetCare.Application.DTOs.Consulta;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Queries;

namespace Clyvo.PetCare.UnitTests.Api;

/// <summary>Preservação de filtros nos links de paginação. Padrão AAA.</summary>
public class QueryStringHelperTests
{
    [Fact]
    public void ParaValoresDeRota_DeveUsarCamelCaseEIgnorarNulos()
    {
        // Arrange
        var query = new ConsultaQueryDto
        {
            Status = StatusConsulta.Realizada,
            PetId = 5,
            DataInicio = new DateTime(2026, 1, 15),
            OrdenarPor = ConsultaOrdenacao.ProximoRetorno,
            Direcao = DirecaoOrdenacao.Asc,
            Pagina = 2,
            TamanhoPagina = 10
        };

        // Act
        var valores = QueryStringHelper.ParaValoresDeRota(query);

        // Assert
        Assert.Equal("Realizada", valores["status"]);
        Assert.Equal("5", valores["petId"]);
        Assert.Equal("2026-01-15T00:00:00.0000000", valores["dataInicio"]);
        Assert.Equal("ProximoRetorno", valores["ordenarPor"]);
        Assert.Equal("Asc", valores["direcao"]);
        Assert.Equal("2", valores["pagina"]);
        Assert.False(valores.ContainsKey("tipo"));
        Assert.False(valores.ContainsKey("dataFim"));
    }
}
