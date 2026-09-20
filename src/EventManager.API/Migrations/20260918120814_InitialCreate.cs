using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventManager.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false),
                    AvailableSeats = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bookings_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "events",
                columns: new[] { "Id", "AvailableSeats", "Description", "EndAt", "StartAt", "Title", "TotalSeats" },
                values: new object[,]
                {
                    { 1, 3, "Annual gathering of tech leaders discussing AI, cloud computing, and the future of software development.", new DateTime(2026, 3, 12, 18, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 10, 9, 0, 0, 0, DateTimeKind.Utc), "Tech Workshop 2026", 3 },
                    { 2, 1000, "One of the six World Marathon Majors, running through the heart of Berlin.", new DateTime(2026, 9, 27, 16, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 27, 8, 30, 0, 0, DateTimeKind.Utc), "Berlin Marathon", 1000 },
                    { 3, 150, "Early-stage founders pitch their ideas to a panel of investors and industry experts.", new DateTime(2026, 4, 15, 21, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 15, 18, 0, 0, 0, DateTimeKind.Utc), "Startup Pitch Night", 150 },
                    { 4, 50, null, new DateTime(2026, 7, 4, 22, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 4, 17, 0, 0, 0, DateTimeKind.Utc), "Jazz in the Park", 50 },
                    { 5, 250, "Black-tie fundraising dinner supporting local children's hospitals.", new DateTime(2026, 11, 21, 23, 59, 0, 0, DateTimeKind.Utc), new DateTime(2026, 11, 21, 19, 0, 0, 0, DateTimeKind.Utc), "Annual Charity Gala", 250 },
                    { 6, 30, "Hands-on workshop covering portrait, landscape, and street photography techniques.", new DateTime(2026, 5, 8, 16, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 8, 10, 0, 0, 0, DateTimeKind.Utc), "Photography Workshop", 30 },
                    { 7, 5000, "Three-day celebration of local cuisine, international wines, and live cooking demonstrations.", new DateTime(2026, 8, 16, 20, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 14, 11, 0, 0, 0, DateTimeKind.Utc), "Food & Wine Festival", 5000 },
                    { 8, 50, "Deep dive into .NET 9, performance tuning, and modern C# patterns for senior developers.", new DateTime(2026, 6, 3, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 3, 9, 0, 0, 0, DateTimeKind.Utc), "C# Advanced Workshop", 50 },
                    { 9, 20, null, new DateTime(2026, 4, 22, 13, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 22, 8, 0, 0, 0, DateTimeKind.Utc), "Community Clean-Up Day", 20 },
                    { 10, 12000, "Over 200 publishers and authors gathering for readings, signings, and panel discussions.", new DateTime(2026, 10, 5, 19, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), "Book Fair 2026", 12000 },
                    { 11, 80, "Monthly UX/UI meetup featuring lightning talks and portfolio reviews.", new DateTime(2026, 6, 18, 21, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 18, 30, 0, 0, DateTimeKind.Utc), "Product Design Meetup", 80 },
                    { 12, 600, "Classic films screened outdoors at the city park. Bring your own blanket.", new DateTime(2026, 7, 25, 23, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 25, 20, 30, 0, 0, DateTimeKind.Utc), "Open Air Cinema Night", 600 },
                    { 13, 250, "48-hour hackathon focused on building software solutions for climate and sustainability challenges.", new DateTime(2026, 9, 7, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 5, 9, 0, 0, 0, DateTimeKind.Utc), "Hackathon: Climate Tech", 250 },
                    { 14, 5, null, new DateTime(2026, 5, 24, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 22, 7, 0, 0, 0, DateTimeKind.Utc), "Yoga & Mindfulness Retreat", 5 },
                    { 15, 2000, "Live orchestral performance and countdown celebration at the city concert hall.", new DateTime(2027, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 12, 31, 20, 0, 0, 0, DateTimeKind.Utc), "New Year's Eve Concert", 2000 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_EventId",
                table: "bookings",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "events");
        }
    }
}
