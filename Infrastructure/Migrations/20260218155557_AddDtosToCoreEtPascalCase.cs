using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDtosToCoreEtPascalCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "state",
                table: "Addresses",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "secondLine",
                table: "Addresses",
                newName: "SecondLine");

            migrationBuilder.RenameColumn(
                name: "postalCode",
                table: "Addresses",
                newName: "PostalCode");

            migrationBuilder.RenameColumn(
                name: "firstLine",
                table: "Addresses",
                newName: "FirstLine");

            migrationBuilder.RenameColumn(
                name: "country",
                table: "Addresses",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "Addresses",
                newName: "City");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "State",
                table: "Addresses",
                newName: "state");

            migrationBuilder.RenameColumn(
                name: "SecondLine",
                table: "Addresses",
                newName: "secondLine");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "Addresses",
                newName: "postalCode");

            migrationBuilder.RenameColumn(
                name: "FirstLine",
                table: "Addresses",
                newName: "firstLine");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "Addresses",
                newName: "country");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Addresses",
                newName: "city");
        }
    }
}
