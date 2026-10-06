using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PROG_Part2_RaceDay_API.Tests.Controllers;

[TestClass]
public class EventsControllerTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [TestInitialize]
    public void Initialize()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [TestMethod]
    public async Task CreateEvent_AsOrganiser_ReturnsCreated()
    {
        await RegisterAndLogin("Organiser");

        var response = await _client.PostAsJsonAsync(
            "/api/events",
            ValidEvent());

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }

    [TestMethod]
    public async Task CreateEvent_AsParticipant_ReturnsForbidden()
    {
        await RegisterAndLogin("Participant");

        var response = await _client.PostAsJsonAsync(
            "/api/events",
            ValidEvent());

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task RegisterAndLogin(string role)
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        const string password = "TestPassword123!";

        var registration = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                firstName = "Test",
                lastName = "User",
                email,
                password,
                role,
                phoneNumber = "0712345678",
                dateOfBirth = new DateOnly(2000, 1, 1)
            });

        Assert.AreEqual(HttpStatusCode.Created, registration.StatusCode);

        var login = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password });

        Assert.AreEqual(HttpStatusCode.OK, login.StatusCode);
    }

    private static object ValidEvent() => new
    {
        name = "Test Community Run",
        description = "A test event for the API.",
        eventDate = new DateOnly(2027, 1, 15),
        venue = "Test Sports Ground",
        city = "Johannesburg",
        province = "Gauteng",
        distanceKm = 5m,
        eventType = "Run"
    };


    [TestMethod]
    public async Task CreateEvent_WithoutLogin_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/events",
            ValidEvent());

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    [TestMethod]
    public async Task UpdateEvent_AsOrganiser_ReturnsOk()
    {
        var eventId = await CreateEventAsOrganiser();

        var response = await _client.PutAsJsonAsync(
            $"/api/events/{eventId}",
            ValidEvent());

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteEvent_AsOrganiser_ReturnsNoContent()
    {
        var eventId = await CreateEventAsOrganiser();

        var response = await _client.DeleteAsync(
            $"/api/events/{eventId}");

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }


    private async Task<int> CreateEventAsOrganiser()
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        const string password = "TestPassword123!";

        var registration = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                firstName = "Test",
                lastName = "Organiser",
                email,
                password,
                role = "Organiser",
                phoneNumber = "0712345678",
                dateOfBirth = new DateOnly(2000, 1, 1)
            });

        Assert.AreEqual(HttpStatusCode.Created, registration.StatusCode);

        var login = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password });

        Assert.AreEqual(HttpStatusCode.OK, login.StatusCode);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/events",
            ValidEvent());

        Assert.AreEqual(HttpStatusCode.Created, createResponse.StatusCode);

        var eventJson = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());

        return eventJson.RootElement
            .GetProperty("eventId")
            .GetInt32();
    }

    


}