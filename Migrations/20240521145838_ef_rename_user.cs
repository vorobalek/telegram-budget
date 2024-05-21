using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBudget.Migrations
{
    /// <inheritdoc />
    public partial class ef_rename_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Users_CreatedBy",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Participating_Users_ParticipantId",
                table: "Participating");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionConfirmations_Users_RecipientId",
                table: "TransactionConfirmations");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_CreatedBy",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Budgets_ActiveBudgetId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameIndex(
                name: "IX_Users_ActiveBudgetId",
                table: "User",
                newName: "IX_User_ActiveBudgetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_User_CreatedBy",
                table: "Budgets",
                column: "CreatedBy",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Participating_User_ParticipantId",
                table: "Participating",
                column: "ParticipantId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionConfirmations_User_RecipientId",
                table: "TransactionConfirmations",
                column: "RecipientId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_User_CreatedBy",
                table: "Transactions",
                column: "CreatedBy",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Budgets_ActiveBudgetId",
                table: "User",
                column: "ActiveBudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_User_CreatedBy",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Participating_User_ParticipantId",
                table: "Participating");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionConfirmations_User_RecipientId",
                table: "TransactionConfirmations");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_User_CreatedBy",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Budgets_ActiveBudgetId",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_User_ActiveBudgetId",
                table: "Users",
                newName: "IX_Users_ActiveBudgetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Users_CreatedBy",
                table: "Budgets",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Participating_Users_ParticipantId",
                table: "Participating",
                column: "ParticipantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionConfirmations_Users_RecipientId",
                table: "TransactionConfirmations",
                column: "RecipientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_CreatedBy",
                table: "Transactions",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Budgets_ActiveBudgetId",
                table: "Users",
                column: "ActiveBudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
