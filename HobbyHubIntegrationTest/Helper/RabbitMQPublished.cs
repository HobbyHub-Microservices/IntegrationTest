using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace HobbyHubIntegrationTest.Helper
{
    
    public class RabbitMQPublished
    {

        private IModel _channel;
        private IConnection _connection;
        
        public void PublishKeycloakMockData(string hostname, int port)
        {
            var factory = new ConnectionFactory()
            {
                HostName = hostname,
                Port = port,
                ClientProvidedName = "MockData",
                UserName = "testuser",
                Password = "testpassword"
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                
                const string queueName = "keycloak.mock.queue";
               
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
                
                _channel.QueueBind(
                    queue: queueName,
                    exchange: "amq.topic",
                    routingKey: "KK.EVENT.ADMIN.HobbyHub.SUCCESS.REGISTER"
                );

                
                Console.WriteLine("--> Connected to RabbitMQ");

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong inside RabbitMQ {ex.ToString()}");
            }

            var messageObject = new
            {
                @class = "com.github.aznamier.keycloak.event.provider.EventClientNotificationMqMsg",
                time = 0101010101010,
                type = "REGISTER",
                realmId = "-",
                clientId = "-",
                userId = "01010101-ffff-0101-ffff-01ff01ff01ff01",
                ipAddress = "github",
                details = new
                {
                    auth_method = "",
                    auth_type = "",
                    register_method = "",
                    last_name = "IntegrationTest",
                    redirect_uri = "github",
                    first_name = "IntegrationTest",
                    code_id = "",
                    email = "IntegrationTest@mail.nl",
                    username = "IntegrationTest"
                }
            };
                
            var serializedMessage = JsonSerializer.Serialize(messageObject);
            var body = Encoding.UTF8.GetBytes(serializedMessage);

                
                
            _channel.BasicPublish(
                exchange: "amq.topic",
                routingKey: "KK.EVENT.ADMIN.HobbyHub.SUCCESS.REGISTER",
                basicProperties: null,
                mandatory: true,
                body: body
            );
            

            Console.WriteLine("--> Message published");
            
 
        }
    }
}
