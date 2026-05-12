using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace foll_backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "care");

            migrationBuilder.CreateTable(
                name: "patients",
                schema: "care",
                columns: table => new
                {
                    patient_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dni = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    blood_type = table.Column<short>(type: "smallint", nullable: false),
                    medical_conditions = table.Column<string>(type: "jsonb", nullable: false),
                    current_guardian_user_id = table.Column<long>(type: "bigint", nullable: true),
                    official_guardian_user_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_patients", x => x.patient_id);
                });

            migrationBuilder.CreateTable(
                name: "relationship_types",
                schema: "care",
                columns: table => new
                {
                    relationship_type_id = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_relationship_types", x => x.relationship_type_id);
                });

            migrationBuilder.CreateTable(
                name: "emergency_contacts",
                schema: "care",
                columns: table => new
                {
                    emergency_contact_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    patient_id = table.Column<long>(type: "bigint", nullable: false),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    relationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_emergency_contacts", x => x.emergency_contact_id);
                    table.ForeignKey(
                        name: "fk_emergency_contacts_patients_patient_id",
                        column: x => x.patient_id,
                        principalSchema: "care",
                        principalTable: "patients",
                        principalColumn: "patient_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_invitations",
                schema: "care",
                columns: table => new
                {
                    patient_invitation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    patient_id = table.Column<long>(type: "bigint", nullable: false),
                    inviting_user_id = table.Column<long>(type: "bigint", nullable: false),
                    relationship_type_id = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_patient_invitations", x => x.patient_invitation_id);
                    table.ForeignKey(
                        name: "fk_patient_invitations_patients_patient_id",
                        column: x => x.patient_id,
                        principalSchema: "care",
                        principalTable: "patients",
                        principalColumn: "patient_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_patients",
                schema: "care",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    patient_id = table.Column<long>(type: "bigint", nullable: false),
                    relationship_type_id = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_patients", x => new { x.patient_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_user_patients_patients_patient_id",
                        column: x => x.patient_id,
                        principalSchema: "care",
                        principalTable: "patients",
                        principalColumn: "patient_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "care",
                table: "relationship_types",
                columns: new[] { "relationship_type_id", "name" },
                values: new object[,]
                {
                    { (short)1, "Hijo" },
                    { (short)2, "Vecino" },
                    { (short)3, "Enfermera" },
                    { (short)4, "Familiar" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_emergency_contacts_patient_id",
                schema: "care",
                table: "emergency_contacts",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "ix_patient_invitations_inviting_user_id",
                schema: "care",
                table: "patient_invitations",
                column: "inviting_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_patient_invitations_patient_id",
                schema: "care",
                table: "patient_invitations",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "ix_patients_dni",
                schema: "care",
                table: "patients",
                column: "dni",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emergency_contacts",
                schema: "care");

            migrationBuilder.DropTable(
                name: "patient_invitations",
                schema: "care");

            migrationBuilder.DropTable(
                name: "relationship_types",
                schema: "care");

            migrationBuilder.DropTable(
                name: "user_patients",
                schema: "care");

            migrationBuilder.DropTable(
                name: "patients",
                schema: "care");
        }
    }
}
