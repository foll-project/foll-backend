using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace foll_backend.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeDeviceManagmentConnectivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "connectivity_status",
                schema: "device",
                table: "devices",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_connectivity_change_at",
                schema: "device",
                table: "devices",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "monitoring_started_at",
                schema: "device",
                table: "devices",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "device",
                table: "devices",
                keyColumn: "device_id",
                keyValue: 1001L,
                columns: new[] { "connectivity_status", "last_connectivity_change_at", "monitoring_started_at" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                schema: "device",
                table: "devices",
                keyColumn: "device_id",
                keyValue: 1002L,
                columns: new[] { "connectivity_status", "last_connectivity_change_at", "monitoring_started_at" },
                values: new object[] { null, null, null });

            migrationBuilder.Sql("""
                UPDATE device.devices
                SET connectivity_status = 1,
                    monitoring_started_at = COALESCE(last_heartbeat_at, NOW()),
                    last_connectivity_change_at = COALESCE(last_heartbeat_at, NOW())
                WHERE assigned_patient_id IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                WITH duplicated_open_events AS (
                    SELECT device_event_id
                    FROM (
                        SELECT device_event_id,
                               ROW_NUMBER() OVER (
                                   PARTITION BY device_id, event_type
                                   ORDER BY created_at DESC, device_event_id DESC
                               ) AS rn
                        FROM device.device_events
                        WHERE is_resolved = false
                    ) ranked
                    WHERE ranked.rn > 1
                )
                UPDATE device.device_events AS target
                SET is_resolved = true,
                    resolved_at = created_at
                WHERE target.device_event_id IN (SELECT device_event_id FROM duplicated_open_events);
                """);

            migrationBuilder.CreateIndex(
                name: "ix_device_events_device_id_event_type",
                schema: "device",
                table: "device_events",
                columns: new[] { "device_id", "event_type" },
                unique: true,
                filter: "\"is_resolved\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_device_events_device_id_event_type",
                schema: "device",
                table: "device_events");

            migrationBuilder.DropColumn(
                name: "connectivity_status",
                schema: "device",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "last_connectivity_change_at",
                schema: "device",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "monitoring_started_at",
                schema: "device",
                table: "devices");
        }
    }
}
