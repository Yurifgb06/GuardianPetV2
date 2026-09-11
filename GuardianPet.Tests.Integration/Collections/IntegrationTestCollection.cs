using GuardianPet.Tests.Integration.Fixtures;
using Xunit;
namespace GuardianPet.Tests.Integration.Collections;
[CollectionDefinition("Oracle integration", DisableParallelization = true)]
public sealed class IntegrationTestCollection : ICollectionFixture<ApiFixture> { }

