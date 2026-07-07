using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace foll_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencySmsAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "emergency_location_access_links",
                schema: "notification",
                columns: table => new
                {
                    emergency_location_access_link_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    incident_key = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<long>(type: "bigint", nullable: false),
                    device_id = table.Column<long>(type: "bigint", nullable: true),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    last_accessed_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_emergency_location_access_links", x => x.emergency_location_access_link_id);
                });

            migrationBuilder.CreateTable(
                name: "sms_notification_logs",
                schema: "notification",
                columns: table => new
                {
                    sms_notification_log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    incident_key = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    emergency_contact_id = table.Column<long>(type: "bigint", nullable: true),
                    patient_id = table.Column<long>(type: "bigint", nullable: false),
                    device_id = table.Column<long>(type: "bigint", nullable: true),
                    recipient_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    notification_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notification_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    location_access_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    provider_message_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sms_notification_logs", x => x.sms_notification_log_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_emergency_location_access_links_expires_at",
                schema: "notification",
                table: "emergency_location_access_links",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_emergency_location_access_links_incident_key",
                schema: "notification",
                table: "emergency_location_access_links",
                column: "incident_key");

            migrationBuilder.CreateIndex(
                name: "ix_emergency_location_access_links_token_hash",
                schema: "notification",
                table: "emergency_location_access_links",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sms_notification_logs_emergency_contact_id",
                schema: "notification",
                table: "sms_notification_logs",
                column: "emergency_contact_id");

            migrationBuilder.CreateIndex(
                name: "ix_sms_notification_logs_incident_key",
                schema: "notification",
                table: "sms_notification_logs",
                column: "incident_key");

            migrationBuilder.CreateIndex(
                name: "ix_sms_notification_logs_incident_key_phone_number",
                schema: "notification",
                table: "sms_notification_logs",
                columns: new[] { "incident_key", "phone_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sms_notification_logs_notification_status",
                schema: "notification",
                table: "sms_notification_logs",
                column: "notification_status");

            migrationBuilder.CreateIndex(
                name: "ix_sms_notification_logs_patient_id",
                schema: "notification",
                table: "sms_notification_logs",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "ix_sms_notification_logs_user_id",
                schema: "notification",
                table: "sms_notification_logs",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emergency_location_access_links",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "sms_notification_logs",
                schema: "notification");
        }
    }
}
