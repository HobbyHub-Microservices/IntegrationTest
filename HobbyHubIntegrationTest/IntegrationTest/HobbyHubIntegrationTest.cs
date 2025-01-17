using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text.Json;
using HobbyHubIntegrationTest.Helper;
using HobbyHubIntegrationTest.Setup;
using Npgsql;
using Xunit.Abstractions;


namespace HobbyHubIntegrationTest.IntegrationTest
{
    [Collection("HobbyHub")]
    [TestCaseOrderer("HobbyHubIntegrationTest.Setup.PriorityOrderer", "HobbyHubIntegrationTest")]
    public class HobbyHubIntegrationTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string JWT_SECRET_KEY = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJYc3kxV3dzZTBhaGZrOHZheDI5V2pOR3luLVJzYmxzdjJjNWlsWnZ1bU1jIn0.eyJleHAiOjE3MzYyNzA5MDUsImlhdCI6MTczNjI3MDYwNSwianRpIjoiZjcxMmVjZjYtOWJhNy00N2MwLThiMzktZDhmY2VhOWVlNjg4IiwiaXNzIjoiaHR0cDovL2tleWNsb2FrLWhvYmJ5aHViLmF1c3RyYWxpYWNlbnRyYWwuY2xvdWRhcHAuYXp1cmUuY29tL3JlYWxtcy9Ib2JieUh1YiIsImF1ZCI6ImFjY291bnQiLCJzdWIiOiI0MGRlNmNhNS1mNmQ0LTRkYjgtYTE5Zi1jNTIxMzFhMDMzNTIiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJ1c2VyLXNlcnZpY2UiLCJzaWQiOiI1NmRjZDAxZi1mNjJiLTRkZmEtYWY3MS05NTdkNzNiZjdhYjgiLCJhY3IiOiIxIiwiYWxsb3dlZC1vcmlnaW5zIjpbImh0dHBzOi8vaG9iYnlodWIuYXVzdHJhbGlhY2VudHJhbC5jbG91ZGFwcC5henVyZS5jb20vKiJdLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsiZGVmYXVsdC1yb2xlcy1ob2JieWh1YiIsIm9mZmxpbmVfYWNjZXNzIiwidW1hX2F1dGhvcml6YXRpb24iXX0sInJlc291cmNlX2FjY2VzcyI6eyJhY2NvdW50Ijp7InJvbGVzIjpbIm1hbmFnZS1hY2NvdW50IiwibWFuYWdlLWFjY291bnQtbGlua3MiLCJ2aWV3LXByb2ZpbGUiXX19LCJzY29wZSI6InByb2ZpbGUgZW1haWwiLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwibmFtZSI6InRlc3QgdGVzdCIsInByZWZlcnJlZF91c2VybmFtZSI6InBvc3RtYW4iLCJnaXZlbl9uYW1lIjoidGVzdCIsImZhbWlseV9uYW1lIjoidGVzdCIsImVtYWlsIjoicG9zdG1hbkBwb3N0bWFuLm5sIn0.OwdnatvORryLz7Y-ikh9ekpOgR4Kbz-HzonNbD6W6XSd0oOUYrUIE86cyNGKNnm0A3fCBg7A7q4ToLR1maXoGwGXOiutcT-mjYPIjevSq5yf5oz-MQec8e4MheFpvfGJOUY1cCxEszZQUNCzifmPyOUF7hmxPft9SowdhkcEHBvvECZ658Ye9dcaZUx5FrZcZPk9WMCCMatxY6zpPZV7fwXUC3n1vdn_B_OS9IZHdeRZgd4lyR_SQTlwg9_mUGkAD-EFRBl0O2Dez4BalOA79rGc82N0g0JBcb_i6lN61VLMPDYm4AQ1HA790ng96ZrpLXyfnMw3g8-ZYX-epwA_VQ";
        private  ContainerSetup _containerSetup;
        private  RabbitMQPublished _rabbitMqPublisher;

        public HobbyHubIntegrationTest(SharedContainerSetup sharedContainerSetup, ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _containerSetup = sharedContainerSetup.ContainerSetup;
            _rabbitMqPublisher = new RabbitMQPublished();
        }

        [Fact, TestPriority(1)]
        public async Task Step1_Test_UserService_Should_Have_New_User_After_Message()
        {
            
            // Use _rabbitMqPublisher to publish a message and verify behavior
            _rabbitMqPublisher.PublishKeycloakMockData(
                _containerSetup._rabbitMqContainer.Hostname,
                _containerSetup._rabbitMqContainer.GetMappedPublicPort(5672)
                );
            
            Console.WriteLine("Message should be send");
            await Task.Delay(TimeSpan.FromSeconds(30));
            
       
            
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWT_SECRET_KEY);
            var response = await client.GetAsync($"http://{_containerSetup._serviceUserContainer.Hostname}:{_containerSetup._serviceUserContainer.GetMappedPublicPort(8080)}/api/Users/test/1");
            Assert.True(response.IsSuccessStatusCode);
            
        }
        
        [Fact, TestPriority(2)]
        
        public async Task Step2_Submit_A_Post_And_Check_If_It_Exists()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWT_SECRET_KEY);

            var postData = new
            {
                title = "Test Post",
                content = "Test Content",
                userId = 1,
                hobbyId = 1,
                
            }; 
            
            var postqueryData = new
            {
                title = "Test Post",
                content = "Test Content",
                userName = "IntegrationTest",
                hobbyName = "HobbyNameTest",
                
            }; 

            var content = new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json");
            var querycontent = new StringContent(JsonSerializer.Serialize(postqueryData), Encoding.UTF8, "application/json");

            // Act
            try
            {
            var postResponse = await client.PostAsync($"http://{_containerSetup._servicePostCommandContainer.Hostname}:{_containerSetup._servicePostCommandContainer.GetMappedPublicPort(8080)}/api/Post/test", content);
            var postqueryResponse = await client.PostAsync($"http://{_containerSetup._servicePostQueryContainer.Hostname}:{_containerSetup._servicePostQueryContainer.GetMappedPublicPort(8080)}/api/query/Post/test/post", querycontent);

            // Assert
        
                Assert.True(postResponse.IsSuccessStatusCode, "The POST request failed.");
                Assert.True(postqueryResponse.IsSuccessStatusCode, "The POST request failed.");

            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine(e.ToString());
                throw;
            }
           
            // Optional: Check if the post exists by making a GET request
            var getResponse = await client.GetAsync($"http://{_containerSetup._servicePostCommandContainer.Hostname}:{_containerSetup._servicePostCommandContainer.GetMappedPublicPort(8080)}/api/Post/test/1"); // Assuming the ID is 1
            Assert.True(getResponse.IsSuccessStatusCode, "The GET request to fetch the post failed.");
            
            var getqueryResponse = await client.GetAsync($"http://{_containerSetup._servicePostQueryContainer.Hostname}:{_containerSetup._servicePostQueryContainer.GetMappedPublicPort(8080)}/api/query/Post/test/1"); // Assuming the ID is 1
            Assert.True(getqueryResponse.IsSuccessStatusCode, "The GET request to fetch the post failed.");
            
        }
   
        
        
        [Fact, TestPriority(3)]
        public async Task Step3_Delete_User_And_Check_If_It_Exists()
        {
            
           
        }
        
        [Fact, TestPriority(4)]
        public async Task Step4_Check_If_Posts_Still_Exists()
        {
            
           
        }
  
        
    }
 
}

