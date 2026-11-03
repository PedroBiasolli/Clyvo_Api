using Clyvo.PetCare.Domain.Queries;

namespace Clyvo.PetCare.UnitTests.Domain;

/// <summary>Normalização de paginação nos Query Objects. Padrão AAA.</summary>
public class FiltroPaginadoTests
{
    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(3, 10, 20)]
    [InlineData(0, 10, 0)]
    [InlineData(-5, 10, 0)]
    public void Deslocamento_DeveConsiderarPaginaETamanho(int pagina, int tamanho, int esperado)
    {
        // Arrange
        var filtro = new TutorFiltro { Pagina = pagina, TamanhoPagina = tamanho };

        // Act
        var deslocamento = filtro.Deslocamento;

        // Assert
        Assert.Equal(esperado, deslocamento);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void TamanhoPagina_ForaDoLimite_DeveVoltarAoPadrao(int tamanho)
    {
        // Act
        var filtro = new PetFiltro { TamanhoPagina = tamanho };

        // Assert
        Assert.Equal(FiltroPaginado.TamanhoPaginaPadrao, filtro.TamanhoPagina);
    }

    [Fact]
    public void Padroes_DeveOrdenarPorNomeAscendente()
    {
        // Act
        var filtro = new TutorFiltro();

        // Assert
        Assert.Equal(1, filtro.Pagina);
        Assert.Equal(TutorOrdenacao.Nome, filtro.OrdenarPor);
        Assert.Equal(DirecaoOrdenacao.Asc, filtro.Direcao);
    }
}
