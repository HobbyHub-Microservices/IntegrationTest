using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using FluentAssertions;
using Npgsql;
using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace HobbyHubIntegrationTest.Setup;

public class ContainerSetup 
{
    public PostgreSqlContainer _postgresContainer { get; private set; }
    public RabbitMqContainer _rabbitMqContainer { get; private set; }
    private KeycloakContainer _keycloakContainer { get; set; }
    private readonly string JWT_SECRET_KEY = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJYc3kxV3dzZTBhaGZrOHZheDI5V2pOR3luLVJzYmxzdjJjNWlsWnZ1bU1jIn0.eyJleHAiOjE3MzYyNzA5MDUsImlhdCI6MTczNjI3MDYwNSwianRpIjoiZjcxMmVjZjYtOWJhNy00N2MwLThiMzktZDhmY2VhOWVlNjg4IiwiaXNzIjoiaHR0cDovL2tleWNsb2FrLWhvYmJ5aHViLmF1c3RyYWxpYWNlbnRyYWwuY2xvdWRhcHAuYXp1cmUuY29tL3JlYWxtcy9Ib2JieUh1YiIsImF1ZCI6ImFjY291bnQiLCJzdWIiOiI0MGRlNmNhNS1mNmQ0LTRkYjgtYTE5Zi1jNTIxMzFhMDMzNTIiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJ1c2VyLXNlcnZpY2UiLCJzaWQiOiI1NmRjZDAxZi1mNjJiLTRkZmEtYWY3MS05NTdkNzNiZjdhYjgiLCJhY3IiOiIxIiwiYWxsb3dlZC1vcmlnaW5zIjpbImh0dHBzOi8vaG9iYnlodWIuYXVzdHJhbGlhY2VudHJhbC5jbG91ZGFwcC5henVyZS5jb20vKiJdLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsiZGVmYXVsdC1yb2xlcy1ob2JieWh1YiIsIm9mZmxpbmVfYWNjZXNzIiwidW1hX2F1dGhvcml6YXRpb24iXX0sInJlc291cmNlX2FjY2VzcyI6eyJhY2NvdW50Ijp7InJvbGVzIjpbIm1hbmFnZS1hY2NvdW50IiwibWFuYWdlLWFjY291bnQtbGlua3MiLCJ2aWV3LXByb2ZpbGUiXX19LCJzY29wZSI6InByb2ZpbGUgZW1haWwiLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwibmFtZSI6InRlc3QgdGVzdCIsInByZWZlcnJlZF91c2VybmFtZSI6InBvc3RtYW4iLCJnaXZlbl9uYW1lIjoidGVzdCIsImZhbWlseV9uYW1lIjoidGVzdCIsImVtYWlsIjoicG9zdG1hbkBwb3N0bWFuLm5sIn0.OwdnatvORryLz7Y-ikh9ekpOgR4Kbz-HzonNbD6W6XSd0oOUYrUIE86cyNGKNnm0A3fCBg7A7q4ToLR1maXoGwGXOiutcT-mjYPIjevSq5yf5oz-MQec8e4MheFpvfGJOUY1cCxEszZQUNCzifmPyOUF7hmxPft9SowdhkcEHBvvECZ658Ye9dcaZUx5FrZcZPk9WMCCMatxY6zpPZV7fwXUC3n1vdn_B_OS9IZHdeRZgd4lyR_SQTlwg9_mUGkAD-EFRBl0O2Dez4BalOA79rGc82N0g0JBcb_i6lN61VLMPDYm4AQ1HA790ng96ZrpLXyfnMw3g8-ZYX-epwA_VQ";
    private IContainer _serviceKeycloakMockContainer;
    public IContainer _serviceUserContainer { get; private set; }
    public IContainer _servicePostQueryContainer { get; private set; }
    public IContainer _serviceHobbyContainer { get; private set; }
    public IContainer _servicePostCommandContainer { get; private set; }
    private INetwork _network;
    

    internal async Task DisposeAsync()
    {
        // Containers stoppen en opruimen
        try
        {
            // Dispose containers first
            if (_serviceUserContainer is not null)
            {
                await _serviceUserContainer.StopAsync();
                await _serviceUserContainer.DisposeAsync();
            }

            if (_serviceHobbyContainer is not null)
            {
                await _serviceHobbyContainer.StopAsync();
                await _serviceHobbyContainer.DisposeAsync();
            }
            
            if (_servicePostQueryContainer is not null)
            {
                await _servicePostQueryContainer.StopAsync();
                await _servicePostQueryContainer.DisposeAsync();
            }
            
            if (_servicePostCommandContainer is not null)
            {
                await _servicePostCommandContainer.StopAsync();
                await _servicePostCommandContainer.DisposeAsync();
            }


            if (_rabbitMqContainer is not null)
            {
                await _rabbitMqContainer.StopAsync();
                await _rabbitMqContainer.DisposeAsync();
            }

            if (_postgresContainer is not null)
            {
                await _postgresContainer.StopAsync();
                await _postgresContainer.DisposeAsync();
            }

            // Dispose the network last
            if (_network is not null)
            {
                await _network.DeleteAsync();
                await _network.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during cleanup: {ex}");
            throw;
        }

    }

    internal async Task StartServicesAsync()
    {
        _network = new NetworkBuilder()
           .WithName("HobbyHub")
           .WithReuse(true)
           .Build();
        
        await _network.CreateAsync();


        // PostgreSQL container
        _postgresContainer = new PostgreSqlBuilder()
            .WithNetwork(_network)
            .WithNetworkAliases("postgres")
            .WithDatabase("Users")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .WithCleanUp(false)
            .Build();
        

        // RabbitMQ container
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithNetwork(_network)
            .WithNetworkAliases("rabbitmq")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .WithCleanUp(false)
            .Build();

        // Start PostgreSQL and RabbitMQ containers
        await _postgresContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        await CreateDatabasesAsync();

        
        Console.Write(_postgresContainer.GetConnectionString());
        // First the userservice will start up and connect to the rabbitmq and postgres
        _serviceUserContainer = new ContainerBuilder()
            .WithImage("janinevansaaze/userservice:latest")
            .WithPortBinding(0, 8080)
            .WithExposedPort(8080)
            .WithNetwork(_network)
            .WithNetworkAliases("userservice")
            .WithEnvironment("POSTGRES_USER", "testuser")
            .WithEnvironment("POSTGRES_HOST", "postgres")
            .WithEnvironment("POSTGRES_PORT", "5432")
            .WithEnvironment("POSTGRES_PASSWORD", "testpassword")
            .WithEnvironment("IntegrationMode", true.ToString())
            .WithEnvironment("RabbitMQHost", "rabbitmq")
            .WithEnvironment("RabbitMQPort", "5672")
            .WithEnvironment("RabbitMQUsername", "testuser")
            .WithEnvironment("RabbitMQPassword", "testpassword")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
            .WithImagePullPolicy(PullPolicy.Always)
            .WithCleanUp(false)
            .Build();

        await _serviceUserContainer.StartAsync();

        _serviceHobbyContainer = new ContainerBuilder()
           .WithImage("janinevansaaze/hobbyservice:latest") // Vervang met je eigen image
           .WithPortBinding(0, 8081)
           .WithExposedPort(8081)
           .WithNetwork(_network)
           .WithNetworkAliases("hobbyservice")
           .WithEnvironment("POSTGRES_USER", "testuser")
           .WithEnvironment("POSTGRES_HOST", "postgres")
           .WithEnvironment("POSTGRES_PORT", "5432")
           .WithEnvironment("POSTGRES_PASSWORD", "testpassword")
           .WithEnvironment("RabbitMQHost", "rabbitmq")
           .WithEnvironment("RabbitMQPort", "5672")
           .WithEnvironment("RabbitMQUsername", "testuser")
           .WithEnvironment("RabbitMQPassword", "testpassword")
           .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
           .WithImagePullPolicy(PullPolicy.Always)
           .WithCleanUp(false)
           .Build();

        await _serviceHobbyContainer.StartAsync();

        _servicePostQueryContainer = new ContainerBuilder()
            .WithImage("janinevansaaze/post_query_service:latest") // Vervang met je eigen image
            .WithPortBinding(0, 8082)
            .WithExposedPort(8082)
            .WithNetwork(_network)
            .WithNetworkAliases("post_query_service")
            .WithEnvironment("POSTGRES_USER", "testuser")
            .WithEnvironment("POSTGRES_HOST", "postgres")
            .WithEnvironment("POSTGRES_PORT", "5432")
            .WithEnvironment("POSTGRES_PASSWORD", "testpassword")
            .WithEnvironment("RabbitMQHost", "rabbitmq")
            .WithEnvironment("RabbitMQPort", "5672")
            .WithEnvironment("RabbitMQUsername", "testuser")
            .WithEnvironment("RabbitMQPassword", "testpassword")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
            .WithImagePullPolicy(PullPolicy.Always)
            .WithCleanUp(false)
            .Build();


        await _servicePostQueryContainer.StartAsync();
            
        _servicePostCommandContainer = new ContainerBuilder()
            .WithImage("janinevansaaze/post_query_service:latest") // Vervang met je eigen image
            .WithPortBinding(0, 8083)
            .WithExposedPort(8083)
            .WithNetwork(_network)
            .WithNetworkAliases("post_query_service")
            .WithEnvironment("POSTGRES_USER", "testuser")
            .WithEnvironment("POSTGRES_HOST", "postgres")
            .WithEnvironment("POSTGRES_PORT", "5432")
            .WithEnvironment("POSTGRES_PASSWORD", "testpassword")
            .WithEnvironment("RabbitMQHost", "rabbitmq")
            .WithEnvironment("RabbitMQPort", "5672")
            .WithEnvironment("RabbitMQUsername", "testuser")
            .WithEnvironment("RabbitMQPassword", "testpassword")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
            .WithImagePullPolicy(PullPolicy.Always)
            .WithCleanUp(false)
            .Build();

        await _servicePostCommandContainer.StartAsync();
        
        //127.0.0.1 voor database, maar docker container heet bijvoorbeeld postgress
        //Pipeline testing: niet gebruik maken van guest guest wachtwoord -> ander wachtwoord en user
        //.WithWaitStragetty(wait.forunixcontainer).untillportisAvailable({PortNummer})
        //With expose port
        //with port binding

 

      //  _servicePostCommandContainer = new ContainerBuilder()
       //     .WithImage("janinevansaaze/post_command_service:latest") // Vervang met je eigen image
       //     .WithEnvironment("ConnectionStrings__Database", _postgresContainer.GetConnectionString())
       //     .WithEnvironment("RabbitMQ__Host", _rabbitMqContainer.Hostname)
      //      .WithEnvironment("RabbitMQ__Port", _rabbitMqContainer.GetMappedPublicPort(5672).ToString())
       //     .WithPortBinding(5053, 80)
      //      .WithCleanUp(false)
        //    .Build();
        

    }

    private async Task CreateDatabasesAsync()
    {
        var connection = _postgresContainer.GetConnectionString();
        const string createDb = @"
            CREATE DATABASE Hobbies;
            CREATE DATABASE Posts;
        ";

        await using var npgsqlConnection = new NpgsqlConnection(connection);
        await npgsqlConnection.OpenAsync();

        await using var command = new NpgsqlCommand(createDb, npgsqlConnection);
        await command.ExecuteNonQueryAsync();
    }
}