using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace foll_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyFallTypesCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_emergency_incidents_device_id",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.DropIndex(
                name: "ix_emergency_incidents_device_id_incident_type",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.CreateTable(
                name: "fall_types",
                schema: "emergency",
                columns: table => new
                {
                    fall_type_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    severity_level = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fall_types", x => x.fall_type_id);
                });

            migrationBuilder.InsertData(
                schema: "emergency",
                table: "fall_types",
                columns: new[] { "fall_type_id", "description", "name", "severity_level" },
                values: new object[,]
                {
                    { (short)1, "Caída hacia adelante detectada por patrón vectorial frontal del dataset SISFALL.", "FRONTAL", (short)1 },
                    { (short)2, "Caída lateral detectada por desplazamiento dominante en eje lateral del dataset SISFALL.", "LATERAL", (short)2 },
                    { (short)3, "Tipo de caída no clasificado o no enviado por el dispositivo/IA.", "UNKNOWN", (short)2 },
                    { (short)4, "Caída hacia atrás detectada por patrón vectorial posterior del dataset SISFALL.", "BACKWARD", (short)1 }
                });

            migrationBuilder.AddColumn<short>(
                name: "fall_type_id",
                schema: "emergency",
                table: "emergency_incidents",
                type: "smallint",
                nullable: false,
                defaultValue: (short)3);

            migrationBuilder.DropColumn(
                name: "incident_type",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.CreateIndex(
                name: "ix_emergency_incidents_device_id",
                schema: "emergency",
                table: "emergency_incidents",
                column: "device_id",
                unique: true,
                filter: "\"status\" = 'Open'");

            migrationBuilder.CreateIndex(
                name: "ix_emergency_incidents_fall_type_id",
                schema: "emergency",
                table: "emergency_incidents",
                column: "fall_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_fall_types_name",
                schema: "emergency",
                table: "fall_types",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_emergency_incidents_fall_types_fall_type_id",
                schema: "emergency",
                table: "emergency_incidents",
                column: "fall_type_id",
                principalSchema: "emergency",
                principalTable: "fall_types",
                principalColumn: "fall_type_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_emergency_incidents_fall_types_fall_type_id",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.DropTable(
                name: "fall_types",
                schema: "emergency");

            migrationBuilder.DropIndex(
                name: "ix_emergency_incidents_device_id",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.DropIndex(
                name: "ix_emergency_incidents_fall_type_id",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.DropColumn(
                name: "fall_type_id",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.AddColumn<string>(
                name: "incident_type",
                schema: "emergency",
                table: "emergency_incidents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

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
        }
    }
}
