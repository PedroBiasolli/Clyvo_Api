using Clyvo.PetCare.Application.DTOs.Tutor;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Application.Services;
using Clyvo.PetCare.Application.Services.Interfaces;
using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Queries;
using Clyvo.PetCare.Domain.Repositories;
using Clyvo.PetCare.UnitTests.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Clyvo.PetCare.UnitTests.Application.Services;

/// <summary>Casos de uso de tutores com repositórios simulados (Moq). Padrão AAA.</summary>
public class TutorServiceTests
{
    private readonly Mock<ITutorRepository> _tutores = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditoriaRegistrador> _auditoria = new();
    private readonly TutorService _sut;

    public TutorServiceTests()
        => _sut = new TutorService(_tutores.Object, _unitOfWork.Object, _auditoria.Object, NullLogger<TutorService>.Instance);

    private static CriarTutorDto NovoDto(string cpf = "123.456.789-01") => new()
    {
        Nome = "Mariana Silva",
        Cpf = cpf,
        Email = "mariana@email.com",
        Telefone = "11987654321",
        Cidade = "São Paulo",
        Uf = "sp",
        AceitaNotificacoes = true
    };

    [Fact]
    public async Task CriarAsync_DadosValidos_DevePersistirConfirmarEAuditar()
    {
        // Arrange
        _tutores.Setup(r => r.ExisteCpfAsync("12345678901", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var resultado = await _sut.CriarAsync(NovoDto());

        // Assert
        Assert.Equal("12345678901", resultado.Cpf);
        Assert.Equal("SP", resultado.Uf);
        _tutores.Verify(r => r.AdicionarAsync(It.Is<Tutor>(t => t.Nome == "Mariana Silva"), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditoria.Verify(a => a.RegistrarAsync("Tutor", It.IsAny<object>(), AcaoAuditoria.Criacao,
            It.IsAny<TutorResponseDto>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_CpfJaCadastrado_DeveLancarConflitoSemPersistir()
    {
        // Arrange
        _tutores.Setup(r => r.ExisteCpfAsync("12345678901", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var acao = () => _sut.CriarAsync(NovoDto());

        // Assert
        await Assert.ThrowsAsync<ConflitoException>(acao);
        _tutores.Verify(r => r.AdicionarAsync(It.IsAny<Tutor>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuscarPorIdAsync_TutorExistente_DeveRetornarDtoComQuantidadeDePets()
    {
        // Arrange
        var tutor = EntityBuilder.NovoTutor(id: 10);
        tutor.Pets.Add(EntityBuilder.NovoPet(tutor, id: 2)); // o EF faria esse vínculo ao carregar o Include
        _tutores.Setup(r => r.ObterPorIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(tutor);

        // Act
        var resultado = await _sut.BuscarPorIdAsync(10);

        // Assert
        Assert.Equal(10, resultado.Id);
        Assert.Equal(1, resultado.QuantidadePets);
    }

    [Fact]
    public async Task BuscarPorIdAsync_TutorInexistente_DeveLancarRecursoNaoEncontrado()
    {
        // Arrange
        _tutores.Setup(r => r.ObterPorIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Tutor?)null);

        // Act
        var acao = () => _sut.BuscarPorIdAsync(99);

        // Assert
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(acao);
    }

    [Fact]
    public async Task BuscarPorCpfAsync_CpfComPontuacao_DeveConsultarSomenteDigitos()
    {
        // Arrange
        _tutores.Setup(r => r.ObterPorCpfAsync("12345678901", It.IsAny<CancellationToken>()))
                .ReturnsAsync(EntityBuilder.NovoTutor());

        // Act
        var resultado = await _sut.BuscarPorCpfAsync("123.456.789-01");

        // Assert
        Assert.Equal("12345678901", resultado.Cpf);
    }

    [Fact]
    public async Task ListarAsync_QuerySemOrdenacao_DeveAplicarPadroesEMontarPagina()
    {
        // Arrange
        TutorFiltro? filtroRecebido = null;
        var tutores = new[] { EntityBuilder.NovoTutor(1), EntityBuilder.NovoTutor(2, cpf: "98765432100") };
        _tutores.Setup(r => r.ListarAsync(It.IsAny<TutorFiltro>(), It.IsAny<CancellationToken>()))
                .Callback<TutorFiltro, CancellationToken>((filtro, _) => filtroRecebido = filtro)
                .ReturnsAsync(new ResultadoPaginado<Tutor>(tutores, TotalRegistros: 25));

        // Act
        var pagina = await _sut.ListarAsync(new TutorQueryDto { Nome = "mar", Pagina = 2, TamanhoPagina = 10 });

        // Assert
        Assert.NotNull(filtroRecebido);
        Assert.Equal("mar", filtroRecebido!.Nome);
        Assert.Equal(TutorOrdenacao.Nome, filtroRecebido.OrdenarPor);
        Assert.Equal(DirecaoOrdenacao.Asc, filtroRecebido.Direcao);
        Assert.Equal(10, filtroRecebido.Deslocamento);

        Assert.Equal(2, pagina.Itens.Count);
        Assert.Equal(25, pagina.TotalRegistros);
        Assert.Equal(3, pagina.TotalPaginas);
        Assert.True(pagina.TemPaginaAnterior);
        Assert.True(pagina.TemProximaPagina);
    }

    [Fact]
    public async Task AtualizarAsync_TutorExistente_DeveAlterarConfirmarEAuditar()
    {
        // Arrange
        var tutor = EntityBuilder.NovoTutor(id: 3);
        _tutores.Setup(r => r.ObterPorIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(tutor);
        var dto = new AtualizarTutorDto { Nome = "Nome Novo", Email = "novo@x.com", Telefone = "11900001111", AceitaNotificacoes = false };

        // Act
        var resultado = await _sut.AtualizarAsync(3, dto);

        // Assert
        Assert.Equal("Nome Novo", resultado.Nome);
        Assert.False(resultado.AceitaNotificacoes);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditoria.Verify(a => a.RegistrarAsync("Tutor", 3, AcaoAuditoria.Atualizacao,
            It.IsAny<object?>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_TutorComPets_DeveLancarRegraDeNegocioSemRemover()
    {
        // Arrange
        var tutor = EntityBuilder.NovoTutor(id: 4);
        _tutores.Setup(r => r.ObterPorIdAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(tutor);
        _tutores.Setup(r => r.PossuiPetsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var acao = () => _sut.RemoverAsync(4);

        // Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(acao);
        _tutores.Verify(r => r.Remover(It.IsAny<Tutor>()), Times.Never);
    }

    [Fact]
    public async Task RemoverAsync_TutorSemPets_DeveRemoverConfirmarEAuditar()
    {
        // Arrange
        var tutor = EntityBuilder.NovoTutor(id: 5);
        _tutores.Setup(r => r.ObterPorIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(tutor);
        _tutores.Setup(r => r.PossuiPetsAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        await _sut.RemoverAsync(5);

        // Assert
        _tutores.Verify(r => r.Remover(tutor), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditoria.Verify(a => a.RegistrarAsync("Tutor", 5, AcaoAuditoria.Remocao,
            It.IsAny<object?>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
