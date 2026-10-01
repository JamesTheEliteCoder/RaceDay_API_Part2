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

        //Enrollmetn mapping to the Enrolments table
        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.ToTable("Enrolments", table =>
                table.HasCheckConstraint(
                    "CK_Enrolments_Status",
                    "[Status] IN ('Confirmed', 'Cancelled')"));

            entity.Property(enrolment => enrolment.EnrolmentDate)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            entity.Property(enrolment => enrolment.Status)
                .HasColumnType("varchar(20)")
                .HasDefaultValue("Confirmed");

            // Prevent duplicate enrolments in the same event category.
            entity.HasIndex(enrolment => new
            {
                enrolment.ParticipantId,
                enrolment.EventId,
                enrolment.CategoryId
            })
                .IsUnique()
                .HasDatabaseName("UQ_Enrolments_Participant_Event_Category");

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(enrolment => enrolment.ParticipantId)
                .HasConstraintName("FK_Enrolments_Users")
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne<Event>()
                .WithMany()
                .HasForeignKey(enrolment => enrolment.EventId)
                .HasConstraintName("FK_Enrolments_Events")
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(enrolment => enrolment.CategoryId)
                .HasConstraintName("FK_Enrolments_Categories")
                .OnDelete(DeleteBehavior.NoAction);
        });



        //Result mapping to the Results table
        modelBuilder.Entity<Result>(entity =>
        {
            entity.ToTable("Results", table =>
                table.HasCheckConstraint(
                    "CK_Results_Position",
                    "[FinishingPosition] > 0"));

            entity.Property(result => result.FinishTime)
                .HasColumnType("time");

            // Each enrolment can have at most one result.
            entity.HasIndex(result => result.EnrolmentId)
                .IsUnique()
                .HasDatabaseName("UQ_Results_Enrolment");

            entity.HasOne<Enrolment>()
                .WithMany()
                .HasForeignKey(result => result.EnrolmentId)
                .HasConstraintName("FK_Results_Enrolments")
                .OnDelete(DeleteBehavior.NoAction);
        });




    }//end of method











}//end of class