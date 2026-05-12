using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace foll_backend.Migrations
{
    /// <inheritdoc />
    public partial class AlignEmergencyAnalyticsFinalModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE emergency.emergency_incidents
                SET cancellation_reason = 'FalsePositive'
                WHERE status = 'FalsePositive';
                """);

            migrationBuilder.Sql("""
                UPDATE emergency.emergency_incidents
                SET status = 'Cancelled'
                WHERE status IN ('CancelledByDeviceUser', 'FalsePositive');
                """);

            migrationBuilder.DropTable(
                name: "emergency_incident_events",
                schema: "emergency");

            migrationBuilder.DropColumn(
                name: "observation_updated_at",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.DropColumn(
                name: "observation_updated_by_user_id",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "emergency",
                columns: table => new
                {
                    emergency_outbox_message_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_on = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    processed_on = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_emergency_outbox_messages", x => x.emergency_outbox_message_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_emergency_outbox_messages_processed_on",
                schema: "emergency",
                table: "outbox_messages",
                column: "processed_on");

            migrationBuilder.Sql("""
                INSERT INTO emergency.outbox_messages (type, payload, occurred_on, processed_on, error, retry_count)
                SELECT type, payload, occurred_on, processed_on, error, retry_count
                FROM device.outbox_messages
                WHERE type LIKE 'emergency-analytics.%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "emergency");

            migrationBuilder.AddColumn<DateTime>(
                name: "observation_updated_at",
                schema: "emergency",
                table: "emergency_incidents",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "observation_updated_by_user_id",
                schema: "emergency",
                table: "emergency_incidents",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "emergency_incident_events",
                schema: "emergency",
                columns: table => new
                {
                    emergency_incident_event_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    device_id = table.Column<long>(type: "bigint", nullable: false),
                    event_payload = table.Column<string>(type: "jsonb", nullable: false),
                    event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    incident_key = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_emergency_incident_events", x => x.emergency_incident_event_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_emergency_incident_events_device_id",
                schema: "emergency",
                table: "emergency_incident_events",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_emergency_incident_events_incident_key",
                schema: "emergency",
                table: "emergency_incident_events",
                column: "incident_key");

            migrationBuilder.CreateIndex(
                name: "ix_emergency_incident_events_patient_id",
                schema: "emergency",
                table: "emergency_incident_events",
                column: "patient_id");
        }
    }
}
