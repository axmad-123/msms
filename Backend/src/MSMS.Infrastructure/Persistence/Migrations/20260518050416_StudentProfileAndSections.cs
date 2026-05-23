using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StudentProfileAndSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "students",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "students",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfBirth",
                table: "students",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SchoolSection",
                table: "classes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "students");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "students");

            migrationBuilder.DropColumn(
                name: "PlaceOfBirth",
                table: "students");

            migrationBuilder.DropColumn(
                name: "SchoolSection",
                table: "classes");
        }
    }
}
