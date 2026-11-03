using System.Text.Json;
using Clyvo.PetCare.Application.DTOs.Auditoria;
using Clyvo.PetCare.Application.DTOs.Auth;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Consulta;
using Clyvo.PetCare.Application.DTOs.Pet;
using Clyvo.PetCare.Application.DTOs.Tutor;
using Clyvo.PetCare.Application.DTOs.Vacina;
using Clyvo.PetCare.Application.DTOs.Veterinario;
using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Queries;

namespace Clyvo.PetCare.Application.Mappings;

/// <summary>
/// Conversões entre entidades de domínio e DTOs. Mantidas num único lugar
/// para que nenhuma entidade "vaze" pela API e os serviços fiquem enxutos.
/// </summary>
public static class DtoMappings
{
    public static PagedResultDto<TDto> ParaPagina<TEntidade, TDto>(
        this ResultadoPaginado<TEntidade> resultado, FiltroPaginado filtro, Func<TEntidade, TDto> mapear) => new()
    {
        Itens = resultado.Itens.Select(mapear).ToList(),
        Pagina = filtro.Pagina,
        TamanhoPagina = filtro.TamanhoPagina,
        TotalRegistros = resultado.TotalRegistros
    };

    // ─────────────────────────────── Tutor ───────────────────────────────

    public static TutorResponseDto ParaDto(this Tutor tutor) => new()
    {
        Id = tutor.Id,
        Nome = tutor.Nome,
        Cpf = tutor.Cpf,
        Email = tutor.Email,
        Telefone = tutor.Telefone,
        Endereco = tutor.Endereco,
        Cidade = tutor.Cidade,
        Uf = tutor.Uf,
        DataCadastro = tutor.DataCadastro,
        AceitaNotificacoes = tutor.AceitaNotificacoes,
        QuantidadePets = tutor.Pets.Count
    };

    public static TutorFiltro ParaFiltro(this TutorQueryDto query) => new()
    {
        Pagina = query.Pagina,
        TamanhoPagina = query.TamanhoPagina,
        Direcao = query.Direcao ?? DirecaoOrdenacao.Asc,
        Nome = query.Nome,
        Cidade = query.Cidade,
        OrdenarPor = query.OrdenarPor ?? TutorOrdenacao.Nome
    };

    // ─────────────────────────────── Pet ───────────────────────────────

    public static PetResponseDto ParaDto(this Pet pet) => new()
    {
        Id = pet.Id,
        Nome = pet.Nome,
        Raca = pet.Raca,
        DataNascimento = pet.DataNascimento,
        IdadeAnos = pet.IdadeAnos,
        Sexo = pet.Sexo,
        PesoKg = pet.PesoKg,
        Porte = pet.Porte,
        Microchip = pet.Microchip,
        Castrado = pet.Castrado,
        Observacoes = pet.Observacoes,
        DataCadastro = pet.DataCadastro,
        TutorId = pet.TutorId,
        TutorNome = pet.Tutor?.Nome
    };

    public static PetFiltro ParaFiltro(this PetQueryDto query) => new()
    {
        Pagina = query.Pagina,
        TamanhoPagina = query.TamanhoPagina,
        Direcao = query.Direcao ?? DirecaoOrdenacao.Asc,
        Nome = query.Nome,
        Raca = query.Raca,
        Porte = query.Porte,
        TutorId = query.TutorId,
        OrdenarPor = query.OrdenarPor ?? PetOrdenacao.Nome
    };

    // ─────────────────────────────── Veterinário ───────────────────────────────

    public static VeterinarioResponseDto ParaDto(this Veterinario veterinario) => new()
    {
        Id = veterinario.Id,
        Nome = veterinario.Nome,
        Crmv = veterinario.Crmv,
        Especialidade = veterinario.Especialidade,
        Email = veterinario.Email,
        Telefone = veterinario.Telefone,
        Ativo = veterinario.Ativo,
        DataCadastro = veterinario.DataCadastro
    };

    public static VeterinarioFiltro ParaFiltro(this VeterinarioQueryDto query) => new()
    {
        Pagina = query.Pagina,
        TamanhoPagina = query.TamanhoPagina,
        Direcao = query.Direcao ?? DirecaoOrdenacao.Asc,
        Nome = query.Nome,
        Especialidade = query.Especialidade,
        Ativo = query.Ativo,
        OrdenarPor = query.OrdenarPor ?? VeterinarioOrdenacao.Nome
    };

    // ─────────────────────────────── Consulta ───────────────────────────────

    public static ConsultaResponseDto ParaDto(this Consulta consulta) => new()
    {
        Id = consulta.Id,
        DataHora = consulta.DataHora,
        Tipo = consulta.Tipo,
        Status = consulta.Status,
        MotivoConsulta = consulta.MotivoConsulta,
        Diagnostico = consulta.Diagnostico,
        Prescricao = consulta.Prescricao,
        Observacoes = consulta.Observacoes,
        ProximoRetorno = consulta.ProximoRetorno,
        ValorCobrado = consulta.ValorCobrado,
        DataCriacao = consulta.DataCriacao,
        PetId = consulta.PetId,
        PetNome = consulta.Pet?.Nome,
        VeterinarioId = consulta.VeterinarioId,
        VeterinarioNome = consulta.Veterinario?.Nome
    };

    public static ConsultaFiltro ParaFiltro(this ConsultaQueryDto query) => new()
    {
        Pagina = query.Pagina,
        TamanhoPagina = query.TamanhoPagina,
        Direcao = query.Direcao ?? DirecaoOrdenacao.Desc,
        Tipo = query.Tipo,
        Status = query.Status,
        PetId = query.PetId,
        VeterinarioId = query.VeterinarioId,
        DataInicio = query.DataInicio,
        DataFim = query.DataFim,
        OrdenarPor = query.OrdenarPor ?? ConsultaOrdenacao.DataHora
    };

    // ─────────────────────────────── Vacina ───────────────────────────────

    public static VacinaResponseDto ParaDto(this Vacina vacina) => new()
    {
        Id = vacina.Id,
        Nome = vacina.Nome,
        DataAplicacao = vacina.DataAplicacao,
        DataProximaDose = vacina.DataProximaDose,
        Lote = vacina.Lote,
        Fabricante = vacina.Fabricante,
        Observacoes = vacina.Observacoes,
        DataRegistro = vacina.DataRegistro,
        PetId = vacina.PetId,
        PetNome = vacina.Pet?.Nome,
        VeterinarioId = vacina.VeterinarioId,
        VeterinarioNome = vacina.Veterinario?.Nome,
        DoseAtrasada = vacina.EstaAtrasada(DateTime.Today)
    };

    public static VacinaFiltro ParaFiltro(this VacinaQueryDto query) => new()
    {
        Pagina = query.Pagina,
        TamanhoPagina = query.TamanhoPagina,
        Direcao = query.Direcao ?? DirecaoOrdenacao.Desc,
        Nome = query.Nome,
        PetId = query.PetId,
        OrdenarPor = query.OrdenarPor ?? VacinaOrdenacao.DataAplicacao
    };

    // ─────────────────────────────── Usuário ───────────────────────────────

    public static UsuarioDto ParaDto(this Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email,
        Perfil = usuario.Perfil,
        Ativo = usuario.Ativo,
        DataCadastro = usuario.DataCadastro
    };

    // ─────────────────────────────── Auditoria ───────────────────────────────

    public static RegistroAuditoriaDto ParaDto(this RegistroAuditoria registro) => new()
    {
        Id = registro.Id ?? string.Empty,
        Entidade = registro.Entidade,
        EntidadeId = registro.EntidadeId,
        Acao = registro.Acao,
        Usuario = registro.Usuario,
        DataHoraUtc = registro.DataHoraUtc,
        CorrelationId = registro.CorrelationId,
        Dados = LerJson(registro.DadosJson)
    };

    public static AuditoriaFiltro ParaFiltro(this AuditoriaQueryDto query) => new()
    {
        Pagina = query.Pagina,
        TamanhoPagina = query.TamanhoPagina,
        Direcao = query.Direcao ?? DirecaoOrdenacao.Desc,
        Entidade = query.Entidade,
        EntidadeId = query.EntidadeId,
        Acao = query.Acao,
        Usuario = query.Usuario,
        DataInicio = query.DataInicio,
        DataFim = query.DataFim
    };

    public static ResumoAuditoriaDto ParaDto(this ResumoAuditoria resumo) => new()
    {
        Entidade = resumo.Entidade,
        Acao = resumo.Acao,
        Total = resumo.Total
    };

    private static JsonElement? LerJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        using var documento = JsonDocument.Parse(json);
        return documento.RootElement.Clone();
    }
}
