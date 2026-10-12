using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManager.EventService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxTopic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "outbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "booking-rejected");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Topic",
                table: "outbox_messages");
        }
    }
}
