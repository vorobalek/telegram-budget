using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TelegramBudget.Migrations
{
    /// <inheritdoc />
    public partial class ef_init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transactions_Versions",
                columns: table => new
                {
                    Number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Serialized = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions_Versions", x => x.Number);
                });

            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ActiveBudgetId = table.Column<Guid>(type: "uuid", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    TimeZone = table.Column<TimeSpan>(type: "interval", nullable: false, defaultValue: new TimeSpan(0, 0, 0, 0, 0))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Budgets_ActiveBudgetId",
                        column: x => x.ActiveBudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Participating",
                columns: table => new
                {
                    ParticipantId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participating", x => new { x.ParticipantId, x.BudgetId });
                    table.ForeignKey(
                        name: "FK_Participating_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participating_Users_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Comment = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    MessageId = table.Column<int>(type: "integer", nullable: false),
                    BudgetId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TransactionConfirmations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<long>(type: "bigint", nullable: false),
                    MessageId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionConfirmations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionConfirmations_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionConfirmations_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CreatedBy",
                table: "Budgets",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_Name_CreatedBy",
                table: "Budgets",
                columns: new[] { "Name", "CreatedBy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participating_BudgetId",
                table: "Participating",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionConfirmations_CreatedAt",
                table: "TransactionConfirmations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionConfirmations_MessageId_RecipientId",
                table: "TransactionConfirmations",
                columns: new[] { "MessageId", "RecipientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionConfirmations_RecipientId",
                table: "TransactionConfirmations",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionConfirmations_TransactionId",
                table: "TransactionConfirmations",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BudgetId",
                table: "Transactions",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedAt",
                table: "Transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedBy",
                table: "Transactions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedBy_MessageId",
                table: "Transactions",
                columns: new[] { "CreatedBy", "MessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Versions_CreatedAt",
                table: "Transactions_Versions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Versions_CreatedBy",
                table: "Transactions_Versions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Versions_EntityId",
                table: "Transactions_Versions",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ActiveBudgetId",
                table: "Users",
                column: "ActiveBudgetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Users_CreatedBy",
                table: "Budgets",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Users_CreatedBy",
                table: "Budgets");

            migrationBuilder.DropTable(
                name: "Participating");

            migrationBuilder.DropTable(
                name: "TransactionConfirmations");

            migrationBuilder.DropTable(
                name: "Transactions_Versions");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Budgets");
        }
    }
}
