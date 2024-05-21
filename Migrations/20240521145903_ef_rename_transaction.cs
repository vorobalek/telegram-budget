using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBudget.Migrations
{
    /// <inheritdoc />
    public partial class ef_rename_transaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionConfirmations_Transactions_TransactionId",
                table: "TransactionConfirmations");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Budgets_BudgetId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_User_CreatedBy",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions_Versions",
                table: "Transactions_Versions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.RenameTable(
                name: "Transactions_Versions",
                newName: "Transaction_Versions");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "Transaction");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_Versions_EntityId",
                table: "Transaction_Versions",
                newName: "IX_Transaction_Versions_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_Versions_CreatedBy",
                table: "Transaction_Versions",
                newName: "IX_Transaction_Versions_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_Versions_CreatedAt",
                table: "Transaction_Versions",
                newName: "IX_Transaction_Versions_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_CreatedBy_MessageId",
                table: "Transaction",
                newName: "IX_Transaction_CreatedBy_MessageId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_CreatedBy",
                table: "Transaction",
                newName: "IX_Transaction_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_CreatedAt",
                table: "Transaction",
                newName: "IX_Transaction_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_BudgetId",
                table: "Transaction",
                newName: "IX_Transaction_BudgetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transaction_Versions",
                table: "Transaction_Versions",
                column: "Number");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Budgets_BudgetId",
                table: "Transaction",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_User_CreatedBy",
                table: "Transaction",
                column: "CreatedBy",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionConfirmations_Transaction_TransactionId",
                table: "TransactionConfirmations",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Budgets_BudgetId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_User_CreatedBy",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionConfirmations_Transaction_TransactionId",
                table: "TransactionConfirmations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transaction_Versions",
                table: "Transaction_Versions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction");

            migrationBuilder.RenameTable(
                name: "Transaction_Versions",
                newName: "Transactions_Versions");

            migrationBuilder.RenameTable(
                name: "Transaction",
                newName: "Transactions");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_Versions_EntityId",
                table: "Transactions_Versions",
                newName: "IX_Transactions_Versions_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_Versions_CreatedBy",
                table: "Transactions_Versions",
                newName: "IX_Transactions_Versions_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_Versions_CreatedAt",
                table: "Transactions_Versions",
                newName: "IX_Transactions_Versions_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_CreatedBy_MessageId",
                table: "Transactions",
                newName: "IX_Transactions_CreatedBy_MessageId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_CreatedBy",
                table: "Transactions",
                newName: "IX_Transactions_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_CreatedAt",
                table: "Transactions",
                newName: "IX_Transactions_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_BudgetId",
                table: "Transactions",
                newName: "IX_Transactions_BudgetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions_Versions",
                table: "Transactions_Versions",
                column: "Number");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionConfirmations_Transactions_TransactionId",
                table: "TransactionConfirmations",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Budgets_BudgetId",
                table: "Transactions",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_User_CreatedBy",
                table: "Transactions",
                column: "CreatedBy",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
