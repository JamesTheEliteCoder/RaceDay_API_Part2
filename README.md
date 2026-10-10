# PROG_Part2_RaceDay_API

# Project Setup
* Created a project in Visual Studio using  ASP.NET Core Web API with the .NET 10 framework.
* Proceeded to create all the folders that I would need.
* Proceeded to create a separate MSTest project and linked it to the current project for unit testing later on.

# Database Design
* The API is being built with an EF Core Code First approach. Its models are based on the Part 1 SQL script, which defines these tables:

Users
Events
Routes
Categories
Enrolments
Results

The current model classes are User, Event, RaceRoute, Category, Enrolment, and Result. The RaceRoute class maps to the SQL table named Routes; its C# name avoids a conflict with an ASP.NET Core routing type.

The RaceDayDbContext includes a DbSet for each model. Mappings have been added for Users, Events, Routes, and Categories, including the relevant SQL column types, check constraints, foreign keys, and delete behaviour. The remaining table mappings and the database migration are still to be completed.

Category creation will also need to check that its selected route belongs to the same event. The SQL script has separate foreign keys for the event and route, so the API must enforce that relationship.

# Local database connection

The development connection uses SQL Server LocalDB and Windows authentication. The connection string is stored under ConnectionStrings: RaceDayDb in appsettings.json.

This is what it looked like:

"ConnectionStrings": {
  "RaceDayDb": "Server=(localdb)\\RaceDay_API_DB;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
}

This connection string is specific to the local development machine My PC). Update the server name if using a different SQL Server instance. Do not put database passwords, API keys, or other secrets in source control.

EF Core migrations will create and update the database from the models. The Part 1 SQL script is a schema reference; this API does not run it manually.

# Packages

I then made use of:
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Tools
Microsoft.EntityFrameworkCore.Design
 to ensure that the database and code communicate as intended.

 # Planned API Functionality
 
The implementation will follow the approved 18-endpoint plan. It covers:

* Registration and login for Organisers and Participants
* Authenticated profile viewing and updates
* Event creation, viewing, updating, and deletion
* Event routes and categories
* Participant enrolments and organiser enrolment views
* Result recording and participant result views

Passwords will be hashed before storage. The Part 1 placeholder password hashes will not be used as login credentials. Session-based authentication and role enforcement are still to be implemented.

 
# CI workflow Screenshot

![Successful GitHub Actions build](docs/images/ci-success.png)


## Technology

- C# and ASP.NET Core Web API on .NET 10
- Entity Framework Core using the Code First approach
- SQL Server LocalDB for local development
- Session-based authentication with role enforcement
- Swagger UI for exploring and testing endpoints
- MSTest for automated tests
- GitHub Actions for continuous integration

## Roles and authentication

Users register as either an **Organiser** or a **Participant**. Passwords are hashed before they are stored. After login, the API keeps the user ID and role in a server-side session.

- **Organisers** can manage their own events, routes, and categories; view enrolments for their events; and record event results.
- **Participants** can view events and categories, enrol in events, and view their own enrolments and results.
- Both roles can view and update their own profiles.
- Registration and login are public. Other protected endpoints require a valid session and the appropriate role.

## API endpoints

| Method | Route | Access |
|---|---|---|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |
| GET | `/api/users/me` | Authenticated user |
| PUT | `/api/users/me` | Authenticated user |
| GET | `/api/events` | Authenticated user |
| POST | `/api/events` | Organiser |
| GET | `/api/events/{id}` | Authenticated user |
| PUT | `/api/events/{id}` | Organiser who manages the event |
| DELETE | `/api/events/{id}` | Organiser who manages the event |
| GET | `/api/events/{eventId}/categories` | Authenticated user |
| POST | `/api/events/{eventId}/categories` | Organiser who manages the event |
| POST | `/api/events/{eventId}/routes` | Organiser who manages the event |
| PUT | `/api/events/{eventId}/routes/{routeId}` | Organiser who manages the event |
| POST | `/api/events/{eventId}/enrolments` | Participant |
| GET | `/api/users/me/enrolments` | Participant |
| GET | `/api/events/{eventId}/enrolments` | Organiser who manages the event |
| POST | `/api/enrolment/{enrolmentId}/result` | Organiser who manages the event |
| GET | `/api/users/me/results` | Participant |

All endpoints are available in Swagger UI with descriptions and request and response information.

## Project structure

- `Controllers` — API endpoint controllers
- `Data` — Entity Framework Core database context
- `DTOs` — API request and response types
- `Filters` — session and role authorisation
- `Models` — database entities
- `Migrations` — EF Core schema migrations
- `PROG_Part2_RaceDay_API.Tests` — automated tests for authentication, events, enrolments, results, and role access

The C# entity `RaceRoute` maps to the SQL table `Routes`. Its class name avoids a naming conflict with ASP.NET Core routing types.

## Database

The API uses Entity Framework Core migrations to create and update the database from the models. Those models and mappings are based on the Part 1 SQL script, including the `Users`, `Events`, `Routes`, `Categories`, `Enrolments`, and `Results` tables.

For local development, the connection string is configured under `ConnectionStrings:RaceDayDb` in `appsettings.json`. Update the LocalDB server name if your SQL Server instance differs.

Example:

```json
"ConnectionStrings": {
  "RaceDayDb": "Server=(localdb)\\RaceDay_API_DB;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
}


