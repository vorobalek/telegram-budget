using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBudget.Migrations
{
    /// <inheritdoc />
    public partial class ef_adjust_naming_convention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budget_User_CreatedBy",
                table: "Budget");

            migrationBuilder.DropForeignKey(
                name: "FK_Participant_Budget_BudgetId",
                table: "Participant");

            migrationBuilder.DropForeignKey(
                name: "FK_Participant_User_UserId",
                table: "Participant");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Budget_BudgetId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_User_CreatedBy",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionConfirmation_Transaction_TransactionId",
                table: "TransactionConfirmation");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionConfirmation_User_RecipientId",
                table: "TransactionConfirmation");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Budget_ActiveBudgetId",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Participant",
                table: "Participant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Budget",
                table: "Budget");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TransactionConfirmation",
                table: "TransactionConfirmation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transaction_Versions",
                table: "Transaction_Versions");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "user");

            migrationBuilder.RenameTable(
                name: "Transaction",
                newName: "transaction");

            migrationBuilder.RenameTable(
                name: "Participant",
                newName: "participant");

            migrationBuilder.RenameTable(
                name: "Budget",
                newName: "budget");

            migrationBuilder.RenameTable(
                name: "TransactionConfirmation",
                newName: "transaction_confirmation");

            migrationBuilder.RenameTable(
                name: "Transaction_Versions",
                newName: "transaction_version");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "user",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TimeZone",
                table: "user",
                newName: "time_zone");

            migrationBuilder.RenameColumn(
                name: "PromptSubject",
                table: "user",
                newName: "prompt_subject");

            migrationBuilder.RenameColumn(
                name: "PromptMessageId",
                table: "user",
                newName: "prompt_message_id");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "user",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "user",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "ActiveBudgetId",
                table: "user",
                newName: "active_budget_id");

            migrationBuilder.RenameIndex(
                name: "IX_User_ActiveBudgetId",
                table: "user",
                newName: "ix_user_active_budget_id");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "transaction",
                newName: "comment");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "transaction",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "transaction",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "transaction",
                newName: "message_id");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "transaction",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "transaction",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BudgetId",
                table: "transaction",
                newName: "budget_id");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_CreatedBy_MessageId",
                table: "transaction",
                newName: "ix_transaction_created_by_message_id");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_CreatedBy",
                table: "transaction",
                newName: "ix_transaction_created_by");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_CreatedAt",
                table: "transaction",
                newName: "ix_transaction_created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_BudgetId",
                table: "transaction",
                newName: "ix_transaction_budget_id");

            migrationBuilder.RenameColumn(
                name: "BudgetId",
                table: "participant",
                newName: "budget_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "participant",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Participant_BudgetId",
                table: "participant",
                newName: "ix_participant_budget_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "budget",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "budget",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "budget",
                newName: "created_by");

            migrationBuilder.RenameIndex(
                name: "IX_Budget_Name_CreatedBy",
                table: "budget",
                newName: "ix_budget_name_created_by");

            migrationBuilder.RenameIndex(
                name: "IX_Budget_CreatedBy",
                table: "budget",
                newName: "ix_budget_created_by");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "transaction_confirmation",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "transaction_confirmation",
                newName: "transaction_id");

            migrationBuilder.RenameColumn(
                name: "RecipientId",
                table: "transaction_confirmation",
                newName: "recipient_id");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "transaction_confirmation",
                newName: "message_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "transaction_confirmation",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionConfirmation_TransactionId",
                table: "transaction_confirmation",
                newName: "ix_transaction_confirmation_transaction_id");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionConfirmation_RecipientId",
                table: "transaction_confirmation",
                newName: "ix_transaction_confirmation_recipient_id");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionConfirmation_MessageId_RecipientId",
                table: "transaction_confirmation",
                newName: "ix_transaction_confirmation_message_id_recipient_id");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionConfirmation_CreatedAt",
                table: "transaction_confirmation",
                newName: "ix_transaction_confirmation_created_at");

            migrationBuilder.RenameColumn(
                name: "Serialized",
                table: "transaction_version",
                newName: "serialized");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "transaction_version",
                newName: "number");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "transaction_version",
                newName: "entity_id");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "transaction_version",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "transaction_version",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_Versions_EntityId",
                table: "transaction_version",
                newName: "ix_transaction_version_entity_id");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_Versions_CreatedBy",
                table: "transaction_version",
                newName: "ix_transaction_version_created_by");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_Versions_CreatedAt",
                table: "transaction_version",
                newName: "ix_transaction_version_created_at");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user",
                table: "user",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_transaction",
                table: "transaction",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_participant",
                table: "participant",
                columns: new[] { "user_id", "budget_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_budget",
                table: "budget",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_transaction_confirmation",
                table: "transaction_confirmation",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_transaction_version",
                table: "transaction_version",
                column: "number");

            migrationBuilder.AddForeignKey(
                name: "fk_budget_user_created_by",
                table: "budget",
                column: "created_by",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_participant_budget_budget_id",
                table: "participant",
                column: "budget_id",
                principalTable: "budget",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_participant_user_user_id",
                table: "participant",
                column: "user_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_transaction_budget_budget_id",
                table: "transaction",
                column: "budget_id",
                principalTable: "budget",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_transaction_user_created_by",
                table: "transaction",
                column: "created_by",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_transaction_confirmation_transaction_transaction_id",
                table: "transaction_confirmation",
                column: "transaction_id",
                principalTable: "transaction",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_transaction_confirmation_user_recipient_id",
                table: "transaction_confirmation",
                column: "recipient_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_budget_active_budget_id",
                table: "user",
                column: "active_budget_id",
                principalTable: "budget",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_budget_user_created_by",
                table: "budget");

            migrationBuilder.DropForeignKey(
                name: "fk_participant_budget_budget_id",
                table: "participant");

            migrationBuilder.DropForeignKey(
                name: "fk_participant_user_user_id",
                table: "participant");

            migrationBuilder.DropForeignKey(
                name: "fk_transaction_budget_budget_id",
                table: "transaction");

            migrationBuilder.DropForeignKey(
                name: "fk_transaction_user_created_by",
                table: "transaction");

            migrationBuilder.DropForeignKey(
                name: "fk_transaction_confirmation_transaction_transaction_id",
                table: "transaction_confirmation");

            migrationBuilder.DropForeignKey(
                name: "fk_transaction_confirmation_user_recipient_id",
                table: "transaction_confirmation");

            migrationBuilder.DropForeignKey(
                name: "fk_user_budget_active_budget_id",
                table: "user");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user",
                table: "user");

            migrationBuilder.DropPrimaryKey(
                name: "pk_transaction",
                table: "transaction");

            migrationBuilder.DropPrimaryKey(
                name: "pk_participant",
                table: "participant");

            migrationBuilder.DropPrimaryKey(
                name: "pk_budget",
                table: "budget");

            migrationBuilder.DropPrimaryKey(
                name: "pk_transaction_version",
                table: "transaction_version");

            migrationBuilder.DropPrimaryKey(
                name: "pk_transaction_confirmation",
                table: "transaction_confirmation");

            migrationBuilder.RenameTable(
                name: "user",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "transaction",
                newName: "Transaction");

            migrationBuilder.RenameTable(
                name: "participant",
                newName: "Participant");

            migrationBuilder.RenameTable(
                name: "budget",
                newName: "Budget");

            migrationBuilder.RenameTable(
                name: "transaction_version",
                newName: "Transaction_Versions");

            migrationBuilder.RenameTable(
                name: "transaction_confirmation",
                newName: "TransactionConfirmation");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "User",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "time_zone",
                table: "User",
                newName: "TimeZone");

            migrationBuilder.RenameColumn(
                name: "prompt_subject",
                table: "User",
                newName: "PromptSubject");

            migrationBuilder.RenameColumn(
                name: "prompt_message_id",
                table: "User",
                newName: "PromptMessageId");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "User",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "User",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "active_budget_id",
                table: "User",
                newName: "ActiveBudgetId");

            migrationBuilder.RenameIndex(
                name: "ix_user_active_budget_id",
                table: "User",
                newName: "IX_User_ActiveBudgetId");

            migrationBuilder.RenameColumn(
                name: "comment",
                table: "Transaction",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "Transaction",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Transaction",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "message_id",
                table: "Transaction",
                newName: "MessageId");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "Transaction",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Transaction",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "budget_id",
                table: "Transaction",
                newName: "BudgetId");

            migrationBuilder.RenameIndex(
                name: "ix_transaction_created_by_message_id",
                table: "Transaction",
                newName: "IX_Transaction_CreatedBy_MessageId");

            migrationBuilder.RenameIndex(
                name: "ix_transaction_created_by",
                table: "Transaction",
                newName: "IX_Transaction_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "ix_transaction_created_at",
                table: "Transaction",
                newName: "IX_Transaction_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_transaction_budget_id",
                table: "Transaction",
                newName: "IX_Transaction_BudgetId");

            migrationBuilder.RenameColumn(
                name: "budget_id",
                table: "Participant",
                newName: "BudgetId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Participant",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "ix_participant_budget_id",
                table: "Participant",
                newName: "IX_Participant_BudgetId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Budget",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Budget",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "Budget",
                newName: "CreatedBy");

            migrationBuilder.RenameIndex(
                name: "ix_budget_name_created_by",
                table: "Budget",
                newName: "IX_Budget_Name_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "ix_budget_created_by",
                table: "Budget",
                newName: "IX_Budget_CreatedBy");

            migrationBuilder.RenameColumn(
                name: "serialized",
                table: "Transaction_Versions",
                newName: "Serialized");

            migrationBuilder.RenameColumn(
                name: "number",
                table: "Transaction_Versions",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "entity_id",
                table: "Transaction_Versions",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "Transaction_Versions",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Transaction_Versions",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_transaction_version_entity_id",
                table: "Transaction_Versions",
                newName: "IX_Transaction_Versions_EntityId");

            migrationBuilder.RenameIndex(
                name: "ix_transaction_version_created_by",
                table: "Transaction_Versions",
                newName: "IX_Transaction_Versions_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "ix_transaction_version_created_at",
                table: "Transaction_Versions",
                newName: "IX_Transaction_Versions_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "TransactionConfirmation",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "transaction_id",
                table: "TransactionConfirmation",
                newName: "TransactionId");

            migrationBuilder.RenameColumn(
                name: "recipient_id",
                table: "TransactionConfirmation",
                newName: "RecipientId");

            migrationBuilder.RenameColumn(
                name: "message_id",
                table: "TransactionConfirmation",
                newName: "MessageId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "TransactionConfirmation",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_transaction_confirmation_transaction_id",
                table: "TransactionConfirmation",
                newName: "IX_TransactionConfirmation_TransactionId");

            migrationBuilder.RenameIndex(
                name: "ix_transaction_confirmation_recipient_id",
                table: "TransactionConfirmation",
                newName: "IX_TransactionConfirmation_RecipientId");

            migrationBuilder.RenameIndex(
                name: "ix_transaction_confirmation_message_id_recipient_id",
                table: "TransactionConfirmation",
                newName: "IX_TransactionConfirmation_MessageId_RecipientId");

            migrationBuilder.RenameIndex(
                name: "ix_transaction_confirmation_created_at",
                table: "TransactionConfirmation",
                newName: "IX_TransactionConfirmation_CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Participant",
                table: "Participant",
                columns: new[] { "UserId", "BudgetId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Budget",
                table: "Budget",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transaction_Versions",
                table: "Transaction_Versions",
                column: "Number");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransactionConfirmation",
                table: "TransactionConfirmation",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Budget_User_CreatedBy",
                table: "Budget",
                column: "CreatedBy",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Participant_Budget_BudgetId",
                table: "Participant",
                column: "BudgetId",
                principalTable: "Budget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Participant_User_UserId",
                table: "Participant",
                column: "UserId",
                principalTable: "User",
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
                name: "FK_Transaction_User_CreatedBy",
                table: "Transaction",
                column: "CreatedBy",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

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

            migrationBuilder.AddForeignKey(
                name: "FK_User_Budget_ActiveBudgetId",
                table: "User",
                column: "ActiveBudgetId",
                principalTable: "Budget",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
