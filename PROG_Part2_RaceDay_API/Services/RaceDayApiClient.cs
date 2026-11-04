using RaceDay_MVC_Part3.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Http;



namespace RaceDay_MVC_Part3.Services;

public class RaceDayApiClient
{// start of class
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public RaceDayApiClient(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    private HttpClient CreateClient()
    {
        var baseUrl = _configuration["ApiSettings:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "The RaceDay API base URL has not been configured.");
        }

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl);

        return client;
    }// end of CreateClient method


    public async Task<ApiUserResponse?> LoginAsync(
        LoginRequest request,
        ISession mvcSession)
    {
        // Create an HTTP client configured with the API base address.
        using var client = CreateClient();

        // Send the credentials to the API's existing login endpoint.
        // The API remains responsible for verifying the password.
        using var response = await client.PostAsJsonAsync(
            "api/auth/login",
            request);

        // If a 401 response is recieved, it means that the credentials were not accepted.
        // Return null so the MVC controller can display a friendly message.
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return null;
        }

        // Surface unexpected API errors rather than treating them as ordinary invalid-credential responses.
        response.EnsureSuccessStatusCode();

        // The API uses a session cookie to recognise authenticated users.
        // Capture the cookie issued by the successful login response.
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            throw new InvalidOperationException(
                "The API login succeeded but no session cookie was returned.");
        }

        // Select the default ASP.NET Core session cookie.
        var sessionCookie = cookies.FirstOrDefault(cookie =>
            cookie.StartsWith(
                ".AspNetCore.Session=",
                StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(sessionCookie))
        {
            throw new InvalidOperationException(
                "The API session cookie could not be found.");
        }

        // Keep only the cookie name and value. Cookie attributes such as
        // Path and HttpOnly belong to the Set-Cookie response header.
        var cookiePair = sessionCookie.Split(';', 2)[0];

        // Store the API cookie in this visitor's server-side MVC session.
        mvcSession.SetString("RaceDayApiSessionCookie", cookiePair);

        // Read the safe user details returned by the API.
        var user = await response.Content.ReadFromJsonAsync<ApiUserResponse>();

        if (user is null)
        {
            // Remove the cookie if the API response was unexpectedly empty.
            mvcSession.Remove("RaceDayApiSessionCookie");

            throw new InvalidOperationException(
                "The API returned an empty login response.");
        }

        return user;
    } // end of LoginAsync method



    // Sends a new user's registration details to the Part 2 API.
    public async Task<bool> RegisterAsync(RegisterViewModel model)
    {
        // Create an API client using the configured base URL.
        var client = _httpClientFactory.CreateClient();

        client.BaseAddress = new Uri(
            _configuration["ApiSettings:BaseUrl"]!);

        // Build the registration request using the API's expected fields.
        var registrationData = new
        {
            model.FirstName,
            model.LastName,
            model.Email,
            model.Password,
            model.Role,
            model.PhoneNumber,
            model.DateOfBirth
        };

        // Submit the registration details to the API.
        var response = await client.PostAsJsonAsync(
            "api/auth/register", registrationData);

        // Return whether the API successfully created the account.
        return response.IsSuccessStatusCode;
    } // end of RegisterAsync method



    public HttpRequestMessage CreateAuthenticatedRequest(
        HttpMethod method,
        string endpoint,
        ISession mvcSession)
    {
        // Create a request for the specified API endpoint.
        var request = new HttpRequestMessage(method, endpoint);

        // Retrieve the API session cookie saved after a successful login.
        var apiCookie = mvcSession.GetString("RaceDayApiSessionCookie");

        if (!string.IsNullOrWhiteSpace(apiCookie))
        {
            // Forward the API cookie so the API can restore its session.
            request.Headers.TryAddWithoutValidation("Cookie", apiCookie);
        }

        return request;
    }



    public async Task<HttpResponseMessage> SendAuthenticatedAsync(
        HttpMethod method,
        string endpoint,
        ISession mvcSession,
        HttpContent? content = null)
    {
        // Prepare the request with the current visitor's API cookie.
        using var request = CreateAuthenticatedRequest(
            method,
            endpoint,
            mvcSession);

        // Attach a request body when the endpoint requires one.
        if (content is not null)
        {
            request.Content = content;
        }

        // Create an HTTP client configured with the Part 2 API address.
        using var client = CreateClient();

        // Send the request and return the response for the caller to inspect.
        return await client.SendAsync(request);
    }// end of SendAuthenticatedAsync method



    /// Requests that the Part 2 API end the current user's session.
    public async Task LogoutAsync(ISession mvcSession)
    {
        // Send the logout request using the saved API session cookie.
        using var response = await SendAuthenticatedAsync(
            HttpMethod.Post,
            "api/auth/logout",
            mvcSession);

        // A successful logout returns HTTP 204 No Content.
        // Other unsuccessful responses should be surfaced to the caller.
        response.EnsureSuccessStatusCode();
    }// end of LogoutAsync method






    //Retrieves events belonging to the currently signed-in organiser.
    public async Task<List<EventViewModel>> GetMyEventsAsync(
        ISession mvcSession)
    {
        // Reuse the authenticated request helper so the API receives
        using var response = await SendAuthenticatedAsync(
            HttpMethod.Get,
            "api/events/mine",
            mvcSession);

      
        response.EnsureSuccessStatusCode();

        // Convert the API's JSON response into MVC event view models.
        var events = await response.Content
            .ReadFromJsonAsync<List<EventViewModel>>();

        // Then return an empty list if the API response contains no JSON body.
        return events ?? new List<EventViewModel>();
    } // end of GetMyEventsAsync method







    // Retrieves all enrolments belonging to the currently signed-in participant.
    public async Task<List<ParticipantEnrolmentViewModel>> GetMyEnrolmentsAsync(
        ISession mvcSession)
    {
        // Send an authenticated request to the participant enrolments endpoint.
        using var response = await SendAuthenticatedAsync(
            HttpMethod.Get,
            "api/enrolments/mine",
            mvcSession);

        // Stop if the API reports an unsuccessful response.
        response.EnsureSuccessStatusCode();

        // Convert the API JSON response into participant enrolment view models.
        var enrolments = await response.Content
            .ReadFromJsonAsync<List<ParticipantEnrolmentViewModel>>();

        // Return an empty list if the API response contains no enrolments.
        return enrolments ?? new List<ParticipantEnrolmentViewModel>();
    } // end of GetMyEnrolmentsAsync method




    // Retrieves all events available for participants to browse.
    public async Task<List<EventViewModel>> GetAvailableEventsAsync(
        ISession mvcSession)
    {
        // Send an authenticated request to retrieve all events.
        var response = await SendAuthenticatedAsync(
            HttpMethod.Get,
            "api/events",
            mvcSession);

        // Throw an exception if the API request was unsuccessful.
        response.EnsureSuccessStatusCode();

        // Convert the API response into MVC event view models.
        return await response.Content.ReadFromJsonAsync<List<EventViewModel>>()
               ?? new List<EventViewModel>();
    } // end of GetAvailableEventsAsync method



    // Retrieves all routes associated with a specific event from the API.
    public async Task<List<RouteViewModel>> GetRoutesAsync(
        int eventId,
        ISession mvcSession)
    {
        // Forward the authenticated API session cookie with the request.
        using var response = await SendAuthenticatedAsync(
            HttpMethod.Get,
            $"api/events/{eventId}/routes",
            mvcSession);

        // Stop if the API reports an unsuccessful response.
        response.EnsureSuccessStatusCode();

        // Convert the API JSON response into a list of route view models.
        var routes = await response.Content.ReadFromJsonAsync<List<RouteViewModel>>();

        // Return an empty list if the API response contains no routes.
        return routes ?? new List<RouteViewModel>();
    }// end of GetRoutesAsync method





    // Retrieves all categories associated with a specific event from the API.
    public async Task<List<CategoryViewModel>> GetCategoriesAsync(
        int eventId,
        ISession mvcSession)
    {
        // Forward the authenticated API session cookie with the request.
        using var response = await SendAuthenticatedAsync(
            HttpMethod.Get,
            $"api/events/{eventId}/categories",
            mvcSession);

        // Stop if the API reports an unsuccessful response.
        response.EnsureSuccessStatusCode();

        // Convert the API JSON response into category view models.
        var categories = await response.Content
            .ReadFromJsonAsync<List<CategoryViewModel>>();

        // Return an empty list if the event has no categories.
        return categories ?? new List<CategoryViewModel>();
    } // end of GetCategoriesAsync method






    // Sends a new route to the API for the specified event.
    public async Task<HttpResponseMessage> CreateRouteAsync(
        CreateRouteViewModel routeModel,
        ISession mvcSession)
    {
        // Prepare the route details expected by the API.
        var routeData = new
        {
            routeModel.RouteName,
            routeModel.AreasCovered,
            routeModel.DistanceKm,
            routeModel.StartLocation,
            routeModel.FinishLocation,
            routeModel.Description,
            routeModel.MapUrl
        };

        // Convert the route details into JSON.
        var content = JsonContent.Create(routeData);

        // Submit the route using the organiser's authenticated API session.
        return await SendAuthenticatedAsync(
            HttpMethod.Post,
            $"api/events/{routeModel.EventId}/routes",
            mvcSession,
            content);
    } // end of CreateRouteAsync method



    // Sends a new category to the API for the specified event.
    public async Task<HttpResponseMessage> CreateCategoryAsync(
        CreateCategoryViewModel categoryModel,
        ISession mvcSession)
    {
        // Prepare only the fields expected by the API.
        var categoryData = new
        {
            categoryModel.Name,
            categoryModel.CategoryType,
            categoryModel.RouteId,
            categoryModel.MinimumAge,
            categoryModel.MaximumAge
        };

        // Convert the category details into JSON.
        var content = JsonContent.Create(categoryData);

        // Submit the category using the organiser's authenticated API session.
        return await SendAuthenticatedAsync(
            HttpMethod.Post,
            $"api/events/{categoryModel.EventId}/categories",
            mvcSession,
            content);
    } // end of CreateCategoryAsync method



    // Retrieves all enrolments for an event managed by the signed-in organiser.
    public async Task<List<EnrolmentViewModel>> GetEventEnrolmentsAsync(
        int eventId,
        ISession mvcSession)
    {
        // Send an authenticated request to the Part 2 API.
        var response = await SendAuthenticatedAsync(
            HttpMethod.Get,
            $"api/events/{eventId}/enrolments",
            mvcSession);

        response.EnsureSuccessStatusCode();

        // Convert the API response into MVC enrolment models.
        return await response.Content.ReadFromJsonAsync<List<EnrolmentViewModel>>()
               ?? new List<EnrolmentViewModel>();
    } // end of GetEventEnrolmentsAsync method






    // Retrieves all recorded results for an event managed by the signed-in organiser.
    public async Task<List<ResultViewModel>> GetEventResultsAsync(
        int eventId,
        ISession mvcSession)
    {
        // Send an authenticated request to the Part 2 API.
        var response = await SendAuthenticatedAsync(
            HttpMethod.Get,
            $"api/events/{eventId}/results",
            mvcSession);

        response.EnsureSuccessStatusCode();

        // Convert the API response into MVC result models.
        return await response.Content.ReadFromJsonAsync<List<ResultViewModel>>()
               ?? new List<ResultViewModel>();
    } //end of GetEventResultsAsync method





    // Records a finishing position and time for an event enrolment.
    public async Task<bool> RecordResultAsync(
        int enrolmentId,
        int finishingPosition,
        TimeOnly finishTime,
        ISession mvcSession)
    {
        // Prepare the result information expected by the API.
        var resultData = new
        {
            FinishingPosition = finishingPosition,
            FinishTime = finishTime
        };

        // Send the result to the API using the organiser's session.
        var response = await SendAuthenticatedAsync(
            HttpMethod.Post,
            $"api/enrolment/{enrolmentId}/result",
            mvcSession,
            JsonContent.Create(resultData));

        return response.IsSuccessStatusCode;
    } // end of RecordResultAsync method







    // Updates an existing result for an event enrolment.
    public async Task<bool> UpdateResultAsync(
        int enrolmentId,
        int finishingPosition,
        TimeOnly finishTime,
        ISession mvcSession)
    {
        // Prepare the updated result information expected by the API.
        var resultData = new
        {
            FinishingPosition = finishingPosition,
            FinishTime = finishTime
        };

        // Send an authenticated PUT request to update the existing result.
        var response = await SendAuthenticatedAsync(
            HttpMethod.Put,
            $"api/enrolment/{enrolmentId}/result",
            mvcSession,
            JsonContent.Create(resultData));

        return response.IsSuccessStatusCode;
    } // end of UpdateResultAsync method



    /// Sends a new event to the Part 2 API for the signed-in organiser.
    public async Task<HttpResponseMessage> CreateEventAsync(
        CreateEventViewModel eventModel,
        ISession mvcSession)
    {
        // Convert the MVC form model into JSON for the API request.
        // The API determines the organiser ID from its own session.
        using var content = JsonContent.Create(new
        {
            eventModel.Name,
            eventModel.Description,
            eventModel.EventDate,
            eventModel.Venue,
            eventModel.City,
            eventModel.Province,
            eventModel.DistanceKm,
            eventModel.EventType
        });

        // Submit the event using the existing authenticated request helper.
        // This forwards the API session cookie saved during login.
        return await SendAuthenticatedAsync(
            HttpMethod.Post,
            "api/events",
            mvcSession,
            content);
    }


    /// Sends an update request for an existing event to the Part 2 API.
    /// Then the API checks that the signed-in organiser owns the event.
    public async Task<HttpResponseMessage> UpdateEventAsync(
        int eventId,
        EditEventViewModel eventModel,
        ISession mvcSession)
    {
        // Send only the fields accepted by the API's UpdateEventRequest.
        using var content = JsonContent.Create(new
        {
            eventModel.Name,
            eventModel.Description,
            eventModel.EventDate,
            eventModel.Venue,
            eventModel.City,
            eventModel.Province,
            eventModel.DistanceKm,
            eventModel.EventType
        });

        // The event ID identifies the resource, then the session cookie authenticates the request.
        return await SendAuthenticatedAsync(
            HttpMethod.Put,
            $"api/events/{eventId}",
            mvcSession,
            content);
    } // end of UpdateEventAsync method






    // Retrieves the details of one event from the Part 2 API.
    public async Task<EventViewModel?> GetEventByIdAsync(
        int eventId,
        ISession mvcSession)
    {
        // Forward the participant's API session cookie with the request.
        var response = await SendAuthenticatedAsync(
            HttpMethod.Get,
            $"api/events/{eventId}",
            mvcSession);

        // Return null if the requested event does not exist.
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EventViewModel>();
    }// end of GetEventByIdAsync method



    // Submits a participant's enrolment for a selected event category.
    public async Task<bool> EnrolInEventAsync(
        int eventId,
        int categoryId,
        ISession mvcSession)
    {
        // Prepare the category information expected by the Part 2 API.
        var enrolmentData = new
        {
            CategoryId = categoryId
        };

        // Send the enrolment request using the participant's API session.
        var response = await SendAuthenticatedAsync(
            HttpMethod.Post,
            $"api/events/{eventId}/enrolments",
            mvcSession,
            JsonContent.Create(enrolmentData));

        // Return whether the API successfully created the enrolment.
        return response.IsSuccessStatusCode;
    } // end of EnrolInEventAsync method











}// end of class
