using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PeopleProfilesClassFinance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "teachers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "parents",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "teachers");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "parents");
        }
    }
}
