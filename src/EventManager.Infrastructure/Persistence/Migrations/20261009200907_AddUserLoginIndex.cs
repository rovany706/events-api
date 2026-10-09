using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLoginIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_users_Login",
                table: "users",
                column: "Login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_Login",
                table: "users");
        }
    }
}
