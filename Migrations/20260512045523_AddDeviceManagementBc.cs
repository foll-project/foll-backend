using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace foll_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceManagementBc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "device");

            migrationBuilder.CreateTable(
                name: "device_events",
                schema: "device",
                columns: table => new
                {
                    device_event_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    device_id = table.Column<long>(type: "bigint", nullable: false),
                    event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    event_payload = table.Column<string>(type: "jsonb", nullable: false),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_events", x => x.device_event_id);
                });

            migrationBuilder.CreateTable(
                name: "devices",
                schema: "device",
                columns: table => new
                {
                    device_id = table.Column<long>(type: "bigint", nullable: false),
                    assigned_patient_id = table.Column<long>(type: "bigint", nullable: true),
                    firmware_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    current_battery_level = table.Column<short>(type: "smallint", nullable: true),
                    is_charging = table.Column<bool>(type: "boolean", nullable: true),
                    last_heartbeat_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_devices", x => x.device_id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "device",
                columns: table => new
                {
                    outbox_message_id = table.Column<long>(type: "bigint", nullable: false)
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
                    table.PrimaryKey("pk_outbox_messages", x => x.outbox_message_id);
                });

            migrationBuilder.InsertData(
                schema: "device",
                table: "devices",
                columns: new[] { "device_id", "assigned_patient_id", "current_battery_level", "firmware_version", "is_charging", "last_heartbeat_at", "status" },
                values: new object[,]
                {
                    { 1001L, null, null, "sim-1.0.0", null, null, (short)1 },
                    { 1002L, null, null, "sim-1.0.0", null, null, (short)1 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_device_events_device_id",
                schema: "device",
                table: "device_events",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_device_events_device_id_event_type_is_resolved",
                schema: "device",
                table: "device_events",
                columns: new[] { "device_id", "event_type", "is_resolved" });

            migrationBuilder.CreateIndex(
                name: "ix_devices_assigned_patient_id",
                schema: "device",
                table: "devices",
                column: "assigned_patient_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_on",
                schema: "device",
                table: "outbox_messages",
                column: "processed_on");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_events",
                schema: "device");

            migrationBuilder.DropTable(
                name: "devices",
                schema: "device");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "device");
        }
    }
}
