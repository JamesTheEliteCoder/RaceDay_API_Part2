using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Models;

namespace PROG_Part2_RaceDay_API.Data;

public class RaceDayDbContext : DbContext
{//start of class
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<RaceRoute> Routes => Set<RaceRoute>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();







    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {//start of method

        

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", table =>
                table.HasCheckConstraint(
                    "CK_Users_Role",
                    "[Role] IN ('Organiser', 'Participant')"));

            entity.Property(user => user.FirstName).HasColumnType("varchar(30)");
            entity.Property(user => user.LastName).HasColumnType("varchar(30)");
            entity.Property(user => user.Email).HasColumnType("varchar(50)");
            entity.Property(user => user.PasswordHash).HasColumnType("varchar(100)");
            entity.Property(user => user.Role).HasColumnType("varchar(20)");
            entity.Property(user => user.PhoneNumber).HasColumnType("varchar(11)");
            entity.Property(user => user.DateOfBirth).HasColumnType("date");
            entity.Property(user => user.CreatedOn)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(user => user.Email).IsUnique();
        });

        //Evenets mapping to to the events table
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events", table =>
            {
                table.HasCheckConstraint(
                    "CK_Events_EventType",
                    "[EventType] IN ('Run', 'Walk', 'Cycle')");

                table.HasCheckConstraint(
                    "CK_Events_Distance",
                    "[DistanceKm] > 0");
            });

            entity.Property(eventItem => eventItem.Name).HasColumnType("varchar(30)");
            entity.Property(eventItem => eventItem.Description).HasColumnType("varchar(500)");
            entity.Property(eventItem => eventItem.EventDate).HasColumnType("date");
            entity.Property(eventItem => eventItem.Venue).HasColumnType("varchar(100)");
            entity.Property(eventItem => eventItem.City).HasColumnType("varchar(50)");
            entity.Property(eventItem => eventItem.Province).HasColumnType("varchar(50)");
            entity.Property(eventItem => eventItem.DistanceKm).HasColumnType("decimal(6,2)");
            entity.Property(eventItem => eventItem.EventType).HasColumnType("nvarchar(30)");
            entity.Property(eventItem => eventItem.CreatedOn)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            // To match the SQL foreign key's default NO ACTION delete behavior
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(eventItem => eventItem.OrganiserId)
                .HasConstraintName("FK_Events_Users")
                .OnDelete(DeleteBehavior.NoAction);
        });


        //RaceRoute mapping to the Routes table

        modelBuilder.Entity<RaceRoute>(entity =>
        {
            // Using C# type avoids a naming conflict, and the database table stays Routes.
            entity.ToTable("Routes", table =>
                table.HasCheckConstraint(
                    "CK_Routes_Distance",
                    "[DistanceKm] > 0"));

            
            entity.Property(route => route.RouteName).HasColumnType("varchar(100)");
            entity.Property(route => route.AreasCovered).HasColumnType("varchar(500)");
            entity.Property(route => route.DistanceKm).HasColumnType("decimal(6,2)");
            entity.Property(route => route.StartLocation).HasColumnType("varchar(150)");
            entity.Property(route => route.FinishLocation).HasColumnType("varchar(150)");
            entity.Property(route => route.Description).HasColumnType("varchar(500)");
            entity.Property(route => route.MapUrl).HasColumnType("varchar(500)");

            // Match the SQL foreign key's default NO ACTION delete behavior.
            entity.HasOne<Event>()
                .WithMany()
                .HasForeignKey(route => route.EventId)
                .HasConstraintName("FK_Routes_Events")
                .OnDelete(DeleteBehavior.NoAction);
        });



        //Category Mapping to the Categories table
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories", table =>
            {
                table.HasCheckConstraint(
                    "CK_Categories_Type",
                    "[CategoryType] IN ('Age', 'Distance')");

                table.HasCheckConstraint(
                    "CK_Categories_Age",
                    "[MinimumAge] IS NULL OR [MaximumAge] IS NULL OR [MinimumAge] <= [MaximumAge]");

                table.HasCheckConstraint(
                    "CK_Categories_MinimumAge",
                    "[MinimumAge] IS NULL OR [MinimumAge] >= 0");

                table.HasCheckConstraint(
                    "CK_Categories_MaximumAge",
                    "[MaximumAge] IS NULL OR [MaximumAge] >= 0");
            });

            entity.Property(category => category.Name).HasColumnType("varchar(100)");
            entity.Property(category => category.CategoryType).HasColumnType("varchar(20)");

            // Because the SQL script has separate event and route foreign keys.
            entity.HasOne<Event>()
                .WithMany()
                .HasForeignKey(category => category.EventId)
                .HasConstraintName("FK_Categories_Events")
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne<RaceRoute>()
                .WithMany()
                .HasForeignKey(category => category.RouteId)
                .HasConstraintName("FK_Categories_Routes")
                .OnDelete(DeleteBehavior.NoAction);
        });






    }//end of method











}//end of class