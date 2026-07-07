using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace foll_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyIncidentAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                schema: "emergency",
                table: "emergency_incidents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                schema: "emergency",
                table: "emergency_incidents");
        }
    }
}
