namespace Clyvo.PetCare.Api.Hateoas;

/// <summary>Gera URLs absolutas para ações dos controllers (base do HATEOAS).</summary>
public interface ILinkService
{
    /// <returns>URL da ação, ou <c>null</c> se a rota não puder ser resolvida.</returns>
    string? Gerar(string acao, string controlador, object? valores = null);
}
