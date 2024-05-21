using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBudget.Migrations
{
    /// <inheritdoc />
    public partial class ef_rename_participant_user_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participant_User_ParticipantId",
                table: "Participant");

            migrationBuilder.RenameColumn(
                name: "ParticipantId",
                table: "Participant",
                newName: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Participant_User_UserId",
                table: "Participant",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participant_User_UserId",
                table: "Participant");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Participant",
                newName: "ParticipantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Participant_User_ParticipantId",
                table: "Participant",
                column: "ParticipantId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
