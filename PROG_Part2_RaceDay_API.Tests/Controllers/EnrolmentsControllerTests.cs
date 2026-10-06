using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PROG_Part2_RaceDay_API.Tests.Controllers;

[TestClass]
public class EnrolmentsControllerTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _organiserClient = null!;
    private HttpClient _participantClient = null!;

    [TestInitialize]
    public void Initialize()
    {
        _factory = new CustomWebApplicationFactory();
        _organiserClient = _factory.CreateClient();
        _participantClient = _factory.CreateClient();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _organiserClient.Dispose();
        _participantClient.Dispose();
        _factory.Dispose();
    }

    [TestMethod]
    public async Task Enrol_AsParticipant_CreatesEnrolment()
    {
        var categoryId = await CreateEventAndCategory();
        await RegisterAndLogin(_participantClient, "Participant");

        var response = await _participantClient.PostAsJsonAsync(
            "/api/events/1/enrolments",
            new { categoryId });

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var myEnrolments = await _participantClient.GetAsync(
            "/api/users/me/enrolments");

        Assert.AreEqual(HttpStatusCode.OK, myEnrolments.StatusCode);

        var body = await myEnrolments.Content.ReadAsStringAsync();
        Assert.IsTrue(body.Contains(categoryId.ToString()));
    }

    [TestMethod]
    public async Task Enrol_AsOrganiser_ReturnsForbidden()
    {
        var categoryId = await CreateEventAndCategory();

        var response = await _organiserClient.PostAsJsonAsync(
            "/api/events/1/enrolments",
            new { categoryId });

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<int> CreateEventAndCategory()
    {
        await RegisterAndLogin(_organiserClient, "Organiser");

        var eventResponse = await _organiserClient.PostAsJsonAsync(
            "/api/events",
            new
            {
                name = "Test Community Run",
                description = "A test event for enrolment.",
                eventDate = new DateOnly(2027, 1, 15),
                venue = "Test Sports Ground",
                city = "Johannesburg",
                province = "Gauteng",
                distanceKm = 5m,
                eventType = "Run"
            });

        Assert.AreEqual(HttpStatusCode.Created, eventResponse.StatusCode);

        var eventJson = JsonDocument.Parse(
            await eventResponse.Content.ReadAsStringAsync());
        var eventId = eventJson.RootElement
            .GetProperty("eventId")
            .GetInt32();

        var routeResponse = await _organiserClient.PostAsJsonAsync(
            $"/api/events/{eventId}/routes",
            new
            {
                routeName = "Test Route",
                areasCovered = "Test Area",
                distanceKm = 5m,
                startLocation = "Test Sports Ground",
                finishLocation = "Test Sports Ground",
                description = "Test route.",
                mapUrl = (string?)null
            });

        Assert.AreEqual(HttpStatusCode.Created, routeResponse.StatusCode);

        var routeJson = JsonDocument.Parse(
            await routeResponse.Content.ReadAsStringAsync());
        var routeId = routeJson.RootElement
            .GetProperty("routeId")
            .GetInt32();

        var categoryResponse = await _organiserClient.PostAsJsonAsync(
            $"/api/events/{eventId}/categories",
            new
            {
                name = "Open 5K",
                categoryType = "Distance",
                routeId,
                minimumAge = (int?)null,
                maximumAge = (int?)null
            });

        Assert.AreEqual(HttpStatusCode.Created, categoryResponse.StatusCode);

        var categoryJson = JsonDocument.Parse(
            await categoryResponse.Content.ReadAsStringAsync());

        return categoryJson.RootElement
            .GetProperty("categoryId")
            .GetInt32();
    }

    private static async Task RegisterAndLogin(HttpClient client, string role)
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        const string password = "TestPassword123!";

        var registration = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                firstName = "Test",
                lastName = role,
                email,
                password,
                role,
                phoneNumber = "0712345678",
                dateOfBirth = new DateOnly(2000, 1, 1)
            });

        Assert.AreEqual(HttpStatusCode.Created, registration.StatusCode);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password });

        Assert.AreEqual(HttpStatusCode.OK, login.StatusCode);
    }
}