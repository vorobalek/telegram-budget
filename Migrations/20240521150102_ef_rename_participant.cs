using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBudget.Migrations
{
    /// <inheritdoc />
    public partial class ef_rename_participant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participating_Budget_BudgetId",
                table: "Participating");

            migrationBuilder.DropForeignKey(
                name: "FK_Participating_User_ParticipantId",
                table: "Participating");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Participating",
                table: "Participating");

            migrationBuilder.RenameTable(
                name: "Participating",
                newName: "Participant");

            migrationBuilder.RenameIndex(
                name: "IX_Participating_BudgetId",
                table: "Participant",
                newName: "IX_Participant_BudgetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Participant",
                table: "Participant",
                columns: new[] { "ParticipantId", "BudgetId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Participant_Budget_BudgetId",
                table: "Participant",
                column: "BudgetId",
                principalTable: "Budget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Participant_User_ParticipantId",
                table: "Participant",
                column: "ParticipantId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participant_Budget_BudgetId",
                table: "Participant");

            migrationBuilder.DropForeignKey(
                name: "FK_Participant_User_ParticipantId",
                table: "Participant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Participant",
                table: "Participant");

            migrationBuilder.RenameTable(
                name: "Participant",
                newName: "Participating");

            migrationBuilder.RenameIndex(
                name: "IX_Participant_BudgetId",
                table: "Participating",
                newName: "IX_Participating_BudgetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Participating",
                table: "Participating",
                columns: new[] { "ParticipantId", "BudgetId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Participating_Budget_BudgetId",
                table: "Participating",
                column: "BudgetId",
                principalTable: "Budget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Participating_User_ParticipantId",
                table: "Participating",
                column: "ParticipantId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
