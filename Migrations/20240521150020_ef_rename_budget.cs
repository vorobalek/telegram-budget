using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBudget.Migrations
{
    /// <inheritdoc />
    public partial class ef_rename_budget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_User_CreatedBy",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Participating_Budgets_BudgetId",
                table: "Participating");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Budgets_BudgetId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Budgets_ActiveBudgetId",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Budgets",
                table: "Budgets");

            migrationBuilder.RenameTable(
                name: "Budgets",
                newName: "Budget");

            migrationBuilder.RenameIndex(
                name: "IX_Budgets_Name_CreatedBy",
                table: "Budget",
                newName: "IX_Budget_Name_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Budgets_CreatedBy",
                table: "Budget",
                newName: "IX_Budget_CreatedBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Budget",
                table: "Budget",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Budget_User_CreatedBy",
                table: "Budget",
                column: "CreatedBy",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Participating_Budget_BudgetId",
                table: "Participating",
                column: "BudgetId",
                principalTable: "Budget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Budget_BudgetId",
                table: "Transaction",
                column: "BudgetId",
                principalTable: "Budget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Budget_ActiveBudgetId",
                table: "User",
                column: "ActiveBudgetId",
                principalTable: "Budget",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budget_User_CreatedBy",
                table: "Budget");

            migrationBuilder.DropForeignKey(
                name: "FK_Participating_Budget_BudgetId",
                table: "Participating");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Budget_BudgetId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Budget_ActiveBudgetId",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Budget",
                table: "Budget");

            migrationBuilder.RenameTable(
                name: "Budget",
                newName: "Budgets");

            migrationBuilder.RenameIndex(
                name: "IX_Budget_Name_CreatedBy",
                table: "Budgets",
                newName: "IX_Budgets_Name_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Budget_CreatedBy",
                table: "Budgets",
                newName: "IX_Budgets_CreatedBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Budgets",
                table: "Budgets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_User_CreatedBy",
                table: "Budgets",
                column: "CreatedBy",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Participating_Budgets_BudgetId",
                table: "Participating",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Budgets_BudgetId",
                table: "Transaction",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Budgets_ActiveBudgetId",
                table: "User",
                column: "ActiveBudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
