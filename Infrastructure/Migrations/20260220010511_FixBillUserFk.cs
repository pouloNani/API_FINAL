using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBillUserFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Shops",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Bills",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestCity",
                table: "Bills",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestCountry",
                table: "Bills",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestEmail",
                table: "Bills",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestFirstName",
                table: "Bills",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestId",
                table: "Bills",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestLastName",
                table: "Bills",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestPhone",
                table: "Bills",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestStreet",
                table: "Bills",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestZipCode",
                table: "Bills",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Bills",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_AppUserId",
                table: "Shops",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_AppUserId",
                table: "Bills",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_UserId",
                table: "Bills",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_AspNetUsers_AppUserId",
                table: "Bills",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_AspNetUsers_UserId",
                table: "Bills",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_AspNetUsers_AppUserId",
                table: "Shops",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_AspNetUsers_AppUserId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_Bills_AspNetUsers_UserId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_Shops_AspNetUsers_AppUserId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Shops_AppUserId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Bills_AppUserId",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_Bills_UserId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "GuestCity",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "GuestCountry",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "GuestEmail",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "GuestFirstName",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "GuestId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "GuestLastName",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "GuestPhone",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "GuestStreet",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "GuestZipCode",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Bills");
        }
    }
}
