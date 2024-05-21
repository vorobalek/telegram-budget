using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBudget.Migrations
{
    /// <inheritdoc />
    public partial class ef_rename_transaction_confirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionConfirmations_Transaction_TransactionId",
                table: "TransactionConfirmations");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionConfirmations_User_RecipientId",
                table: "TransactionConfirmations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TransactionConfirmations",
                table: "TransactionConfirmations");

            migrationBuilder.RenameTable(
                name: "TransactionConfirmations",
                newName: "TransactionConfirmation");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionConfirmations_TransactionId",
                table: "TransactionConfirmation",
                newName: "IX_TransactionConfirmation_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionConfirmations_RecipientId",
                table: "TransactionConfirmation",
                newName: "IX_TransactionConfirmation_RecipientId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionConfirmations_MessageId_RecipientId",
                table: "TransactionConfirmation",
                newName: "IX_TransactionConfirmation_MessageId_RecipientId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionConfirmations_CreatedAt",
                table: "TransactionConfirmation",
                newName: "IX_TransactionConfirmation_CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransactionConfirmation",
                table: "TransactionConfirmation",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionConfirmation_Transaction_TransactionId",
                table: "TransactionConfirmation",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionConfirmation_User_RecipientId",
                table: "TransactionConfirmation",
                column: "RecipientId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionConfirmation_Transaction_TransactionId",
                table: "TransactionConfirmation");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionConfirmation_User_RecipientId",
                table: "TransactionConfirmation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TransactionConfirmation",
                table: "TransactionConfirmation");

            migrationBuilder.RenameTable(
                name: "TransactionConfirmation",
                newName: "TransactionConfirmations");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionConfirmation_TransactionId",
                table: "TransactionConfirmations",
                newName: "IX_TransactionConfirmations_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionConfirmation_RecipientId",
                table: "TransactionConfirmations",
                newName: "IX_TransactionConfirmations_RecipientId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionConfirmation_MessageId_RecipientId",
                table: "TransactionConfirmations",
                newName: "IX_TransactionConfirmations_MessageId_RecipientId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionConfirmation_CreatedAt",
                table: "TransactionConfirmations",
                newName: "IX_TransactionConfirmations_CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransactionConfirmations",
                table: "TransactionConfirmations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionConfirmations_Transaction_TransactionId",
                table: "TransactionConfirmations",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionConfirmations_User_RecipientId",
                table: "TransactionConfirmations",
                column: "RecipientId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
