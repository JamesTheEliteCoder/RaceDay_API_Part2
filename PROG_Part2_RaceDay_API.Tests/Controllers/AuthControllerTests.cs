using System.Net;
using System.Net.Http.Json;
using PROG_Part2_RaceDay_API.Tests;

namespace PROG_Part2_RaceDay_API.Tests.Controllers;

[TestClass]
public class AuthControllerTests
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
    public async Task Register_ValidParticipant_ReturnsCreatedWithoutPasswordHash()
    {
        var email = $"p-{Guid.NewGuid():N}@test.com";

        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            ValidRegistration(email));

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.IsFalse(
            responseBody.Contains("passwordHash", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task Register_InvalidRole_ReturnsBadRequest()
    {
        var request = new
        {
            firstName = "Test",
            lastName = "User",
            email = $"i-{Guid.NewGuid():N}@test.com",
            password = "TestPassword123!",
            role = "InvalidRole",
            phoneNumber = "0712345678",
            dateOfBirth = new DateOnly(2000, 1, 1)
        };

        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var email = $"d-{Guid.NewGuid():N}@test.com";

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            ValidRegistration(email));

        var secondResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            ValidRegistration(email));

        Assert.AreEqual(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    private static object ValidRegistration(string email)
    {
        return new
        {
            firstName = "Test",
            lastName = "Participant",
            email,
            password = "TestPassword123!",
            role = "Participant",
            phoneNumber = "0712345678",
            dateOfBirth = new DateOnly(2000, 1, 1)
        };
    }


    [TestMethod]
    public async Task Login_ValidCredentials_CreatesSession()
    {
        var email = $"l-{Guid.NewGuid():N}@test.com";
        const string password = "TestPassword123!";

        var registration = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                firstName = "Test",
                lastName = "Participant",
                email,
                password,
                role = "Participant",
                phoneNumber = "0712345678",
                dateOfBirth = new DateOnly(2000, 1, 1)
            });

        Assert.AreEqual(HttpStatusCode.Created, registration.StatusCode);

        var login = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password });

        Assert.AreEqual(HttpStatusCode.OK, login.StatusCode);

        // The same client should send the session cookie on this protected request.
        var profile = await _client.GetAsync("/api/users/me");

        Assert.AreEqual(HttpStatusCode.OK, profile.StatusCode);
    }

    [TestMethod]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = $"missing-{Guid.NewGuid():N}@test.com",
                password = "WrongPassword123!"
            });

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }



}