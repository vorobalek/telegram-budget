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
                name: "transaction_version",
                columns: table => new
                {
                    number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    serialized = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction_version", x => x.number);
                });

            migrationBuilder.CreateTable(
                name: "budget",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_budget", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    time_zone = table.Column<TimeSpan>(type: "interval", nullable: false, defaultValue: new TimeSpan(0, 0, 0, 0, 0)),
                    prompt_message_id = table.Column<int>(type: "integer", nullable: true),
                    prompt_subject = table.Column<int>(type: "integer", nullable: true),
                    active_budget_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_budget_active_budget_id",
                        column: x => x.active_budget_id,
                        principalTable: "budget",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "participant",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    budget_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_participant", x => new { x.user_id, x.budget_id });
                    table.ForeignKey(
                        name: "fk_participant_budget_budget_id",
                        column: x => x.budget_id,
                        principalTable: "budget",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_participant_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    comment = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    message_id = table.Column<int>(type: "integer", nullable: false),
                    budget_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_transaction_budget_budget_id",
                        column: x => x.budget_id,
                        principalTable: "budget",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transaction_user_created_by",
                        column: x => x.created_by,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "transaction_confirmation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction_confirmation", x => x.id);
                    table.ForeignKey(
                        name: "fk_transaction_confirmation_transaction_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transaction_confirmation_user_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_budget_created_by",
                table: "budget",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_budget_name_created_by",
                table: "budget",
                columns: new[] { "name", "created_by" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_participant_budget_id",
                table: "participant",
                column: "budget_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_budget_id",
                table: "transaction",
                column: "budget_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_created_at",
                table: "transaction",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_created_by",
                table: "transaction",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_created_by_message_id",
                table: "transaction",
                columns: new[] { "created_by", "message_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transaction_confirmation_created_at",
                table: "transaction_confirmation",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_confirmation_message_id_recipient_id",
                table: "transaction_confirmation",
                columns: new[] { "message_id", "recipient_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transaction_confirmation_recipient_id",
                table: "transaction_confirmation",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_confirmation_transaction_id",
                table: "transaction_confirmation",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_version_created_at",
                table: "transaction_version",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_version_created_by",
                table: "transaction_version",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_version_entity_id",
                table: "transaction_version",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_active_budget_id",
                table: "user",
                column: "active_budget_id");

            migrationBuilder.AddForeignKey(
                name: "fk_budget_user_created_by",
                table: "budget",
                column: "created_by",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_budget_user_created_by",
                table: "budget");

            migrationBuilder.DropTable(
                name: "participant");

            migrationBuilder.DropTable(
                name: "transaction_confirmation");

            migrationBuilder.DropTable(
                name: "transaction_version");

            migrationBuilder.DropTable(
                name: "transaction");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "budget");
        }
    }
}
