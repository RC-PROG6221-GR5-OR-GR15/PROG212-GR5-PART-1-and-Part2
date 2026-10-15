using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marathon.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    UserAccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.UserAccountId);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinAge = table.Column<int>(type: "int", nullable: false),
                    MaxAge = table.Column<int>(type: "int", nullable: false),
                    GenderRestrictions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxParticipants = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "enrolments",
                columns: table => new
                {
                    EnrolmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    enrolment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    enrolment_status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    payment_status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    amount_paid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    paid_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrolments", x => x.EnrolmentId);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Now = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Venue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistanceKm = table.Column<float>(type: "real", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventFee = table.Column<float>(type: "real", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "participant_Events",
                columns: table => new
                {
                    Participant_EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    bib_number = table.Column<int>(type: "int", nullable: false),
                    fining_position = table.Column<int>(type: "int", nullable: false),
                    finish_time_seconds = table.Column<TimeOnly>(type: "time", nullable: false),
                    entry_status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    entered_at = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participant_Events", x => x.Participant_EventId);
                });

            migrationBuilder.CreateTable(
                name: "results",
                columns: table => new
                {
                    ResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    finish_position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    finish_time_seconds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    pace_pace_km = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    overall_rank = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    prize_money = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    category_rank = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_results", x => x.ResultId);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "userRoles",
                columns: table => new
                {
                    userrole_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    assinged_by = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userRoles", x => x.userrole_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.UserId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "enrolments");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "participant_Events");

            migrationBuilder.DropTable(
                name: "results");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "userRoles");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
