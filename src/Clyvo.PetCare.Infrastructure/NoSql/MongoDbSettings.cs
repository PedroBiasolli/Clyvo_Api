namespace Clyvo.PetCare.Infrastructure.NoSql;

/// <summary>Configuração da conexão com o MongoDB (seção <c>MongoDb</c> do appsettings).</summary>
public sealed class MongoDbSettings
{
    public const string Secao = "MongoDb";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "clyvo_petcare";
    public string ColecaoAuditoria { get; set; } = "auditoria";

    /// <summary>Tempo máximo para seleção de servidor/conexão. Curto para não travar requisições se o Mongo cair.</summary>
    public int TimeoutSegundos { get; set; } = 3;
}
