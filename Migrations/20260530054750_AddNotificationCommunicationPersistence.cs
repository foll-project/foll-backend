using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace foll_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationCommunicationPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "notification");

            migrationBuilder.CreateTable(
                name: "notification_logs",
                schema: "notification",
                columns: table => new
                {
                    notification_log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    notification_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notification_channel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    notification_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    data_json = table.Column<string>(type: "jsonb", nullable: true),
                    provider_message_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    device_event_id = table.Column<long>(type: "bigint", nullable: true),
                    patient_id = table.Column<long>(type: "bigint", nullable: true),
                    device_id = table.Column<long>(type: "bigint", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    read_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    acknowledged_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_logs", x => x.notification_log_id);
                });

            migrationBuilder.CreateTable(
                name: "user_push_tokens",
                schema: "notification",
                columns: table => new
                {
                    user_push_token_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    platform = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    device_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_used_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_push_tokens", x => x.user_push_token_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notification_logs_created_at",
                schema: "notification",
                table: "notification_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_notification_logs_device_id",
                schema: "notification",
                table: "notification_logs",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_logs_notification_status",
                schema: "notification",
                table: "notification_logs",
                column: "notification_status");

            migrationBuilder.CreateIndex(
                name: "ix_notification_logs_patient_id",
                schema: "notification",
                table: "notification_logs",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_logs_user_id",
                schema: "notification",
                table: "notification_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_push_tokens_user_id",
                schema: "notification",
                table: "user_push_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_push_tokens_user_id_token",
                schema: "notification",
                table: "user_push_tokens",
                columns: new[] { "user_id", "token" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_logs",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "user_push_tokens",
                schema: "notification");
        }
    }
}
