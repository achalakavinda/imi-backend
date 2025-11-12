using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MigratingAssistant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRevokedTokensTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RevokedTokens_Token",
                table: "RevokedTokens");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RevokedTokens",
                type: "varchar(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(2048)",
                oldMaxLength: 2048)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RevokedTokens_Token",
                table: "RevokedTokens",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RevokedTokens_Token",
                table: "RevokedTokens");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RevokedTokens",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(512)",
                oldMaxLength: 512)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RevokedTokens_Token",
                table: "RevokedTokens",
                column: "Token");
        }
    }
}
