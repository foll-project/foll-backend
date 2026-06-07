using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace foll_backend.Migrations
{
    /// <inheritdoc />
    public partial class MergeCareNotificationModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_emergency_contacts_patient_patient_id",
                schema: "care",
                table: "emergency_contacts");

            migrationBuilder.DropForeignKey(
                name: "fk_emergency_incidents_fall_type_fall_type_id",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.DropForeignKey(
                name: "fk_patient_annotations_patient_patient_id",
                schema: "care",
                table: "patient_annotations");

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

            migrationBuilder.AddForeignKey(
                name: "fk_patient_annotations_patients_patient_id",
                schema: "care",
                table: "patient_annotations",
                column: "patient_id",
                principalSchema: "care",
                principalTable: "patients",
                principalColumn: "patient_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_emergency_contacts_patients_patient_id",
                schema: "care",
                table: "emergency_contacts");

            migrationBuilder.DropForeignKey(
                name: "fk_emergency_incidents_fall_types_fall_type_id",
                schema: "emergency",
                table: "emergency_incidents");

            migrationBuilder.DropForeignKey(
                name: "fk_patient_annotations_patients_patient_id",
                schema: "care",
                table: "patient_annotations");

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

            migrationBuilder.AddForeignKey(
                name: "fk_patient_annotations_patient_patient_id",
                schema: "care",
                table: "patient_annotations",
                column: "patient_id",
                principalSchema: "care",
                principalTable: "patients",
                principalColumn: "patient_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
