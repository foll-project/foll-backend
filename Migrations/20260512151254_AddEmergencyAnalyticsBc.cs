using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace foll_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyAnalyticsBc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "emergency");

            migrationBuilder.CreateTable(
                name: "emergency_incident_events",
                schema: "emergency",
                columns: table => new
                {
                    emergency_incident_event_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    incident_key = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<long>(type: "bigint", nullable: false),
                    patient_id = table.Column<long>(type: "bigint", nullable: false),
                    event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    event_payload = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_emergency_incident_events", x => x.emergency_incident_event_id);
                });

            migrationBuilder.CreateTable(
                name: "emergency_incidents",
                schema: "emergency",
                columns: table => new
                {
                    emergency_incident_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    incident_key = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<long>(type: "bigint", nullable: false),
                    patient_id = table.Column<long>(type: "bigint", nullable: false),
                    incident_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    opened_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    last_signal_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    cancelled_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    ai_confidence_score = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    last_source_payload = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_emergency_incidents", x => x.emergency_incident_id);
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

            migrationBuilder.CreateIndex(
                name: "ix_emergency_incidents_device_id",
                schema: "emergency",
                table: "emergency_incidents",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_emergency_incidents_device_id_incident_type",
                schema: "emergency",
                table: "emergency_incidents",
                columns: new[] { "device_id", "incident_type" },
                unique: true,
                filter: "\"status\" = 'Open'");

            migrationBuilder.CreateIndex(
                name: "ix_emergency_incidents_incident_key",
                schema: "emergency",
                table: "emergency_incidents",
                column: "incident_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_emergency_incidents_patient_id",
                schema: "emergency",
                table: "emergency_incidents",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "ix_emergency_incidents_status",
                schema: "emergency",
                table: "emergency_incidents",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emergency_incident_events",
                schema: "emergency");

            migrationBuilder.DropTable(
                name: "emergency_incidents",
                schema: "emergency");
        }
    }
}
