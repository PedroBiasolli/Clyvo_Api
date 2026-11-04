using System.Text.Json;
using System.Text.Json.Serialization;
using Clyvo.PetCare.Application.Abstractions;
using Clyvo.PetCare.Application.Services.Interfaces;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Clyvo.PetCare.Application.Services;

/// <summary>
/// Grava eventos na trilha de auditoria (MongoDB). A gravação é tolerante a
/// falhas: a operação de negócio já foi confirmada no banco relacional e não
/// deve ser revertida por indisponibilidade do armazenamento NoSQL.
/// </summary>
public sealed class AuditoriaRegistrador : IAuditoriaRegistrador
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IAuditoriaRepository _repositorio;
    private readonly IUsuarioAtual _usuarioAtual;
    private readonly ILogger<AuditoriaRegistrador> _logger;

    public AuditoriaRegistrador(
        IAuditoriaRepository repositorio,
        IUsuarioAtual usuarioAtual,
        ILogger<AuditoriaRegistrador> logger)
    {
        _repositorio = repositorio;
        _usuarioAtual = usuarioAtual;
        _logger = logger;
    }

    public async Task RegistrarAsync(
        string entidade, object entidadeId, AcaoAuditoria acao, object? dados,
        string? executadoPor = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var registro = RegistroAuditoria.Criar(
                entidade,
                Convert.ToString(entidadeId, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                acao,
                executadoPor ?? _usuarioAtual.Identificador,
                _usuarioAtual.CorrelationId,
                dados is null ? null : JsonSerializer.Serialize(dados, dados.GetType(), JsonOptions));

            await _repositorio.InserirAsync(registro, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex,
                "Falha ao registrar auditoria de {Acao} em {Entidade} {EntidadeId}",
                acao, entidade, entidadeId);
        }
    }
}
