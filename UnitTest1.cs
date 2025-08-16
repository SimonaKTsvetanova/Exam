using System.Net;
using System.Text.Json;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;




namespace Exam
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string createdFoodId;
        private const string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("SimonaTsvet", "Simo1234");
            var option = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(option);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });
            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;

        }

        [Test, Order(1)]
        public void CreateFood_ShouldReturnCreated()
        {
            var food = new
            {
                name = "New food",
                Description = "Delicious new food item",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create");
            request.AddJsonBody(food);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            createdFoodId = json.GetProperty("foodId").GetString() ?? string.Empty;

            Assert.That(createdFoodId, Is.Not.Null.And.Not.Empty, "Food ID should not be null or emty.");

        }

        [Test, Order(2)]

        public void EditFoodTitle_ShouldReturnOk()
        {
            var changes = new[]
            {
                new {path = "/name", op = "replace", value = "Updated food name"}

            };

            var request = new RestRequest($"/api/Food/Edit/{createdFoodId}", Method.Patch);

            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllFoods_ShouldReturnList()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var foods = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(foods, Is.Not.Empty);

        }
        [Test, Order(4)]
        public void DeleteFood_ShouldReturnOK()
        {
            var request = new RestRequest($"/api/Food/Delete/{createdFoodId}", Method.Delete);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("No food revues"));
        }
        [Test, Order(5)]
        public void CreateFoodWithoutRequiredFields_ShouldReturnBadRequest()
        {
            var food = new
            {
                Name = "",
                Description = "",
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingFood_SholdReturnNotFound()
        {
            string fakeId = "123";
            var changes = new[]
            {
                new{ path = "/name", op = "replace", value = "New title"}
            };
            var request = new RestRequest($"/api/Food/Edit/{fakeId}", Method.Patch);
            request.AddJsonBody(changes);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No food revues"));


        }
        [Test, Order(7)]
        public void DeletingNonExistingFood_ShouldReturnBadRequest()
        {
            string fakeId = "123";

            var request = new RestRequest($"/api/Food/Delete/{fakeId}", Method.Delete);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this food revue"));
        }



        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}