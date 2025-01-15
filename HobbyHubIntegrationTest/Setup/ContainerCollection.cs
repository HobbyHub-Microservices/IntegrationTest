namespace HobbyHubIntegrationTest.Setup;

[CollectionDefinition("HobbyHub", DisableParallelization = true)]
public class ContainerCollection : ICollectionFixture<SharedContainerSetup>
{
    // This class does not need any code; it is only for xUnit to link the SharedContainerSetup fixture to the collection.
}