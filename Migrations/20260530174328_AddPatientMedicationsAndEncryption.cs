using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace foll_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientMedicationsAndEncryption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_emergency_contacts_patients_patient_id",
                schema: "care",
                table: "emergency_contacts");

            migrationBuilder.DropForeignKey(
                name: "fk_emergency_incidents_fall_types_fall_type_id",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.AlterColumn<string>(
                name: "medical_conditions",
                schema: "care",
                table: "patients",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AddColumn<string>(
                name: "medications",
                schema: "care",
                table: "patients",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "fk_emergency_contacts_patient_patient_id",
                schema: "care",
                table: "emergency_contacts",
                column: "patient_id",
                principalSchema: "care",
                principalTable: "patients",
                principalColumn: "patient_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_emergency_incidents_fall_type_fall_type_id",
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
                name: "fk_emergency_contacts_patient_patient_id",
                schema: "care",
                table: "emergency_contacts");

            migrationBuilder.DropForeignKey(
                name: "fk_emergency_incidents_fall_type_fall_type_id",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.DropColumn(
                name: "medications",
                schema: "care",
                table: "patients");

            migrationBuilder.AlterColumn<string>(
                name: "medical_conditions",
                schema: "care",
                table: "patients",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "fk_emergency_contacts_patients_patient_id",
                schema: "care",
                table: "emergency_contacts",
                column: "patient_id",
                principalSchema: "care",
                principalTable: "patients",
                principalColumn: "patient_id",
                onDelete: ReferentialAction.Cascade);

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
    }
}
