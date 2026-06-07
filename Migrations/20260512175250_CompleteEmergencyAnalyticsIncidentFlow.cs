using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace foll_backend.Migrations
{
    /// <inheritdoc />
    public partial class CompleteEmergencyAnalyticsIncidentFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE emergency.emergency_incidents
                SET status = 'CancelledByDeviceUser'
                WHERE status = 'CancelledByUser';
                """);

            migrationBuilder.Sql("""
                UPDATE emergency.emergency_incident_events
                SET event_type = 'IncidentCancelledByDeviceUser'
                WHERE event_type = 'IncidentCancelledByUser';
                """);

            migrationBuilder.AddColumn<DateTime>(
                name: "closed_at",
                schema: "emergency",
                table: "emergency_incidents",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "closed_by_user_id",
                schema: "emergency",
                table: "emergency_incidents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "final_observation",
                schema: "emergency",
                table: "emergency_incidents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

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

            migrationBuilder.Sql("""
                UPDATE emergency.emergency_incidents
                SET closed_at = COALESCE(cancelled_at, resolved_at),
                    observation_updated_at = COALESCE(cancelled_at, resolved_at)
                WHERE status <> 'Open';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE emergency.emergency_incidents
                SET status = 'CancelledByUser'
                WHERE status = 'CancelledByDeviceUser';
                """);

            migrationBuilder.Sql("""
                UPDATE emergency.emergency_incident_events
                SET event_type = 'IncidentCancelledByUser'
                WHERE event_type = 'IncidentCancelledByDeviceUser';
                """);

            migrationBuilder.DropColumn(
                name: "closed_at",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.DropColumn(
                name: "closed_by_user_id",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.DropColumn(
                name: "final_observation",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.DropColumn(
                name: "observation_updated_at",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.DropColumn(
                name: "observation_updated_by_user_id",
                schema: "emergency",
                table: "emergency_incidents");
        }
    }
}
