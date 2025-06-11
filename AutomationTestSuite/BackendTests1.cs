using NUnit.Framework;
using RestSharp;
using System;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace AutomationTestSuite
{
    [TestFixture]
    public class BackendTests1
    {
        private RestClient client;
        private const string BaseUrl = "https://jsonplaceholder.typicode.com";

        [SetUp]
        public void Setup()
        {
            client = new RestClient(BaseUrl);
        }

        [Test]
        public void VerifyUserWithUsernameExists()
        {
            string test_user = "Samantha";
            //string test_user = "Xipetotec"; //to test a potential fail

            var request = new RestRequest("/users", Method.Get);
            var response = client.Execute(request);
            Assert.That(response.IsSuccessful, Is.True, "API call to /users failed.");

            JArray users = JArray.Parse(response.Content);
            bool userExists = false;

            foreach (JToken user in users)
            {
                if (user["username"]?.ToString() == test_user) 
                {
                    userExists = true;
                    Console.WriteLine("A user with username '" +test_user+ "' was found.");
                    break;
                }
            }

            Assert.That(userExists, Is.True, "User with username '" + test_user + "' was not found.");
        }

        [Test]
        public void CreateNewPost()
        {
            var request = new RestRequest("/posts", Method.Post);
            request.AddJsonBody(new
            {
                title = "My Test Post",
                body = "This is a test post created via the API.",
                userId = 3
            });

            var response = client.Execute(request);

            Assert.That(response.IsSuccessful, Is.True, "Failed to create a new post.");

            JObject ? result = JObject.Parse(response.Content);

            Assert.That(result["title"]?.ToString(), Is.EqualTo("My Test Post"), "Post title did not match.");
            //Assert.That(result["title"]?.ToString(), Is.EqualTo("Some Other Post"), "Post title did not match."); //to test a potential fail
            Assert.That(result["body"]?.ToString(), Is.EqualTo("This is a test post created via the API."), "Post body did not match.");
            //Assert.That(result["body"]?.ToString(), Is.EqualTo("This is a test post, but I don't know how it was created."), "Post body did not match."); //to test a potential fail
            Assert.That((int?)result["userId"], Is.EqualTo(3), "User ID did not match.");
            //Assert.That((int?)result["userId"], Is.EqualTo(4), "User ID did not match."); //to test a potential fail

            if (response.IsSuccessful)
                Console.WriteLine("Successfully created a new post: " + result["title"]?.ToString());
        }

        [Test]
        public void PostsEndpointResponseTime()
        {
            int threshold = 500;
            var request = new RestRequest("/posts", Method.Get);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var response = client.Execute(request);

            stopwatch.Stop();
            var responseTimeMs = stopwatch.ElapsedMilliseconds;

            Console.WriteLine($"The response time of {responseTimeMs}ms did not exceed the expected threshold of " +threshold+"ms");
            Assert.That(responseTimeMs, Is.LessThanOrEqualTo(500), $"Response time {responseTimeMs}ms exceeded threshold of " + threshold + " ms.");
            Assert.That(response.IsSuccessful, Is.True, "Request to /posts failed.");
        }

        [TearDown]
        public void TearDown()
        {
            client?.Dispose();
        }
    }
}

