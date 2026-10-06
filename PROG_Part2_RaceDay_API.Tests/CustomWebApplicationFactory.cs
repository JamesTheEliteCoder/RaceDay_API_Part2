using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PROG_Part2_RaceDay_API.Data;

namespace PROG_Part2_RaceDay_API.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Each factory gets its own database; all requests in that test share it.
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the API's SQL Server configuration before adding InMemory.
            services.RemoveAll<RaceDayDbContext>();
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<RaceDayDbContext>>();

            services.AddDbContext<RaceDayDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}