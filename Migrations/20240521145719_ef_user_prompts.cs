using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBudget.Migrations
{
    /// <inheritdoc />
    public partial class ef_user_prompts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PromptMessageId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PromptSubject",
                table: "Users",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PromptMessageId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PromptSubject",
                table: "Users");
        }
    }
}
