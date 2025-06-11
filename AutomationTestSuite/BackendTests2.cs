using NUnit.Framework;
using RestSharp;
using Newtonsoft.Json;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.DevTools.V135.Audits;
using System.Reflection.Emit;
using Microsoft.VisualBasic;

namespace AutomationTestSuite
{
    [TestFixture]
    public class BackendTests2
    {
        private const string BaseUrl = "https://albums-collection-service.herokuapp.com";
        private RestClient client;
        private long? createdAlbumId;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(BaseUrl);
        }

        public void CreateAlbum()
        {
            var request = new RestRequest("/albums", Method.Post);
            request.AddJsonBody(new
            {
                title = "Greatest Hits", //Greatest Power Ballads
                artist = "The Best Band",
                label = "Odd Man Out",
                genre = "Rock",
                year = 2023,
                songs = 12
            });

            var response = client.Execute(request);

            //for temporary debugging
            //Console.WriteLine("Status code: " + response.StatusCode + "\n\n");
            //Console.WriteLine("Content: \n" + response.Content + "\n\n");
            //Console.WriteLine("Error message: \n" + response.ErrorMessage + "\n");

            Assert.That(response.IsSuccessful, Is.True, "Failed to create album.");

            JObject album = JObject.Parse(response.Content);
            if (album["message"]?.ToString() != null)
            {
                Console.WriteLine(album["message"]?.ToString() + " with id: " + (long?)album["album_id"] + "\n");
                createdAlbumId = (long?)album["album_id"]; //an example of a generated Id: 1749572901163
            }

            Assert.That(createdAlbumId.HasValue, Is.True, "Album ID not returned.");
        }

        public void ValidateAlbum(long? albumId, string title, string artist, string label, string genre, int year, int songs)
        {
            var request = new RestRequest($"/albums/{albumId}", Method.Get);

            var response = client.Execute(request);
            Assert.That(response.IsSuccessful, Is.True, "Failed to retrieve album.");

            JObject album = JObject.Parse(response.Content);         
            Assert.That(album["title"]?.ToString(), Is.EqualTo(title), "Album title did not match.");
            Assert.That(album["artist"]?.ToString(), Is.EqualTo(artist), "Album artist did not match.");
            Assert.That(album["label"]?.ToString(), Is.EqualTo(label), "Album label did not match.");
            Assert.That(album["genre"]?.ToString(), Is.EqualTo(genre), "Album genre did not match.");
            
            if(year == 0) 
            {
                Assert.That(album["year"]?.Type, Is.EqualTo(JTokenType.Null), "Album year is not null.");
            }
            else
            {
                Assert.That((int?)album["year"], Is.EqualTo(year), "Album year did not match.");
            }

            if (songs == 0)
            {
                Assert.That(album["year"]?.Type, Is.EqualTo(JTokenType.Null), "Album songs is not null.");
            }
            else
            {
                Assert.That((int?)album["songs"], Is.EqualTo(songs), "Album songs did not match.");
            }

            

            foreach (JProperty property in album.Properties())
            {
              Console.WriteLine(property.Name + ": " + property.Value);               
            }

            Console.WriteLine("\n");
        }

         public void UpdateAlbum(long? albumId)
        {
            var request = new RestRequest($"/albums/{albumId}", Method.Patch);
            request.AddJsonBody(new
            {
                title = "Ultimate Hits",
                year = 2024,
                songs = 10
            });

            var response = client.Execute(request);
            Assert.That(response.IsSuccessful, Is.True, "Failed to update album.");

            Console.WriteLine("The update request was successful\n");
        }

        public void DeleteProperties(long? albumId)
        {
            var request = new RestRequest($"/albums/{albumId}", Method.Get);

            var response = client.Execute(request);
            Assert.That(response.IsSuccessful, Is.True, "Failed to retrieve album.");

            JObject album = JObject.Parse(response.Content);
            album.Remove("genre");
            album.Remove("year");

            var updateRequest = new RestRequest($"/albums/{albumId}", Method.Put);
            updateRequest.AddStringBody(album.ToString(), DataFormat.Json);

            //attempt to clean up json from its empty properties
            //var settings = new JsonSerializerSettings
            //{
            //    NullValueHandling = NullValueHandling.Ignore
            //};
            //string cleanJson = JsonConvert.SerializeObject(album, settings);
            //updateRequest.AddStringBody(cleanJson, DataFormat.Json);

            var updateResponse = client.Execute(updateRequest);
            Assert.That(updateResponse.IsSuccessful, Is.True, "Failed to delete properties.");

            Console.WriteLine("Deleting properties was successful\n");
        }
            
        public void DeleteAlbum()
        {
            var request = new RestRequest($"/albums/{createdAlbumId}", Method.Delete);
            var response = client.Execute(request);
            Assert.That(response.IsSuccessful, Is.True, "Failed to delete album.");
            Console.WriteLine("Call to delete the album was successful\n");
        }

        public void ValidateAlbumDeletion()
        {
            var request = new RestRequest($"/albums/{createdAlbumId}", Method.Get);
            var response = client.Execute(request);
            Assert.That((int)response.StatusCode, Is.EqualTo(404), "Album still exists after deletion.");
            Console.WriteLine("The album with id " + createdAlbumId +" doesn't exist any more\n");
        }

        [Test]
        public void FullAlbumLifecycleTestSuite()
        {
            CreateAlbum();
            Console.WriteLine("*Validating after creation:");
            ValidateAlbum(createdAlbumId, "Greatest Hits", "The Best Band", "Odd Man Out", "Rock",2023,12);
            
            UpdateAlbum(createdAlbumId);
            Console.WriteLine("*Validating after updating title, year and songs:");
            ValidateAlbum(createdAlbumId, "Ultimate Hits", "The Best Band", "Odd Man Out", "Rock", 2024, 10);

            DeleteProperties(createdAlbumId);
            ValidateAlbum(createdAlbumId, "Ultimate Hits", "The Best Band", "Odd Man Out", "", 0, 10);

            DeleteAlbum();
            ValidateAlbumDeletion();
        }

        [TearDown]
        public void TearDown()
        {
            client?.Dispose();
        }
    }
}



