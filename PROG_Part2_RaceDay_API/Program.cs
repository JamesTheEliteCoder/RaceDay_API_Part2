using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Data;
using PROG_Part2_RaceDay_API.Models;
using PROG_Part2_RaceDay_API.Services;


var builder = WebApplication.CreateBuilder(args);


// Register the RaceDay database context with the SQL Server
builder.Services.AddDbContext<RaceDayDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDayDb")));

// Hash passwords before they are stored.
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Store session values on the server during development.
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Azure Blob Storage using the connection string stored in User Secrets.
builder.Services.AddSingleton(sp =>
{
    var connectionString = builder.Configuration["AzureBlobStorage:ConnectionString"];

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Azure Blob Storage connection string is not configured.");
    }

    return new BlobServiceClient(connectionString);
}); //end of Azure Blob Storage registration


// Register the Blob Storage service implementation.
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
// end of Blob Storage service registration.


builder.Services.AddAuthorization();
// Add services to the container.
builder.Services.AddControllers();


builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "RaceDay API v1");
    });
}

app.UseHttpsRedirection();
app.UseSession();
app.UseAuthorization();
app.MapControllers();
app.Run();
public partial class Program { }