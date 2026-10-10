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
