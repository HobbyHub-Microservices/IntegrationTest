using HobbyHubIntegrationTest.Helper;

namespace HobbyHubIntegrationTest.Setup;

public class SharedContainerSetup : IAsyncLifetime
{
    public ContainerSetup ContainerSetup { get; private set; }

    private bool _isInitialized = false;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        ContainerSetup = new ContainerSetup();
        await ContainerSetup.StartServicesAsync();
        _isInitialized = true;
    }

    public async Task DisposeAsync()
    {
        await ContainerSetup.DisposeAsync();
    }



    // public class SharedContainerSetup : IAsyncLifetime
    // {
    //     public ContainerSetup ContainerSetup { get; private set; }
    //
    //     private bool _isInitialized = false;
    //
    //     public async Task InitializeAsync()
    //     {
    //         if (_isInitialized) return;
    //
    //         ContainerSetup = new ContainerSetup();
    //         await ContainerSetup.StartServicesAsync();
    //         _isInitialized = true;
    //     }
    //
    //     public async Task DisposeAsync()
    //     {
    //         await ContainerSetup.DisposeAsync();
    //     }
    // }
}