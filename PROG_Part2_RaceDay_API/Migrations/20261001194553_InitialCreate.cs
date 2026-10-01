using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROG_Part2_RaceDay_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "varchar(30)", nullable: false),
                    LastName = table.Column<string>(type: "varchar(30)", nullable: false),
                    Email = table.Column<string>(type: "varchar(50)", nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(100)", nullable: false),
                    Role = table.Column<string>(type: "varchar(20)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(11)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.CheckConstraint("CK_Users_Role", "[Role] IN ('Organiser', 'Participant')");
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganiserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Venue = table.Column<string>(type: "varchar(100)", nullable: false),
                    City = table.Column<string>(type: "varchar(50)", nullable: false),
                    Province = table.Column<string>(type: "varchar(50)", nullable: false),
                    DistanceKm = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                    table.CheckConstraint("CK_Events_Distance", "[DistanceKm] > 0");
                    table.CheckConstraint("CK_Events_EventType", "[EventType] IN ('Run', 'Walk', 'Cycle')");
                    table.ForeignKey(
                        name: "FK_Events_Users",
                        column: x => x.OrganiserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    RouteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    RouteName = table.Column<string>(type: "varchar(100)", nullable: false),
                    AreasCovered = table.Column<string>(type: "varchar(500)", nullable: false),
                    DistanceKm = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    StartLocation = table.Column<string>(type: "varchar(150)", nullable: false),
                    FinishLocation = table.Column<string>(type: "varchar(150)", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", nullable: true),
                    MapUrl = table.Column<string>(type: "varchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.RouteId);
                    table.CheckConstraint("CK_Routes_Distance", "[DistanceKm] > 0");
                    table.ForeignKey(
                        name: "FK_Routes_Events",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId");
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    CategoryType = table.Column<string>(type: "varchar(20)", nullable: false),
                    MinimumAge = table.Column<int>(type: "int", nullable: true),
                    MaximumAge = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                    table.CheckConstraint("CK_Categories_Age", "[MinimumAge] IS NULL OR [MaximumAge] IS NULL OR [MinimumAge] <= [MaximumAge]");
                    table.CheckConstraint("CK_Categories_MaximumAge", "[MaximumAge] IS NULL OR [MaximumAge] >= 0");
                    table.CheckConstraint("CK_Categories_MinimumAge", "[MinimumAge] IS NULL OR [MinimumAge] >= 0");
                    table.CheckConstraint("CK_Categories_Type", "[CategoryType] IN ('Age', 'Distance')");
                    table.ForeignKey(
                        name: "FK_Categories_Events",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId");
                    table.ForeignKey(
                        name: "FK_Categories_Routes",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "RouteId");
                });

            migrationBuilder.CreateTable(
                name: "Enrolments",
                columns: table => new
                {
                    EnrolmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantId = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    EnrolmentDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "Confirmed")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrolments", x => x.EnrolmentId);
                    table.CheckConstraint("CK_Enrolments_Status", "[Status] IN ('Confirmed', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_Enrolments_Categories",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId");
                    table.ForeignKey(
                        name: "FK_Enrolments_Events",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId");
                    table.ForeignKey(
                        name: "FK_Enrolments_Users",
                        column: x => x.ParticipantId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    ResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrolmentId = table.Column<int>(type: "int", nullable: false),
                    FinishingPosition = table.Column<int>(type: "int", nullable: false),
                    FinishTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.ResultId);
                    table.CheckConstraint("CK_Results_Position", "[FinishingPosition] > 0");
                    table.ForeignKey(
                        name: "FK_Results_Enrolments",
                        column: x => x.EnrolmentId,
                        principalTable: "Enrolments",
                        principalColumn: "EnrolmentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_EventId",
                table: "Categories",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_RouteId",
                table: "Categories",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_CategoryId",
                table: "Enrolments",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_EventId",
                table: "Enrolments",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "UQ_Enrolments_Participant_Event_Category",
                table: "Enrolments",
                columns: new[] { "ParticipantId", "EventId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_OrganiserId",
                table: "Events",
                column: "OrganiserId");

            migrationBuilder.CreateIndex(
                name: "UQ_Results_Enrolment",
                table: "Results",
                column: "EnrolmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_EventId",
                table: "Routes",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "Enrolments");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
