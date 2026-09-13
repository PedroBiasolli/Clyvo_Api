using Clyvo.PetCare.IntegrationTests.Fixtures;

namespace Clyvo.PetCare.IntegrationTests.Collections;

/// <summary>
/// Collection Fixture do xUnit. Todos os testes que declararem
/// <c>[Collection("Api")]</c> compartilham a mesma instância de
/// <see cref="ClyvoWebApplicationFactory"/> — o host é iniciado uma única
/// vez para todo o grupo, o que reduz drasticamente o tempo total da suíte.
/// </summary>
[CollectionDefinition(Name)]
public class ApiTestCollection : ICollectionFixture<ClyvoWebApplicationFactory>
{
    public const string Name = "Api";
}
