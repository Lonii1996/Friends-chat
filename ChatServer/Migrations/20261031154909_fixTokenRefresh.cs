using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.Migrations
{
    /// <inheritdoc />
    public partial class fixTokenRefresh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TokenHesh",
                table: "UsersRefreshToken",
                newName: "TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_UsersRefreshToken_TokenHesh",
                table: "UsersRefreshToken",
                newName: "IX_UsersRefreshToken_TokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TokenHash",
                table: "UsersRefreshToken",
                newName: "TokenHesh");

            migrationBuilder.RenameIndex(
                name: "IX_UsersRefreshToken_TokenHash",
                table: "UsersRefreshToken",
                newName: "IX_UsersRefreshToken_TokenHesh");
        }
    }
}
