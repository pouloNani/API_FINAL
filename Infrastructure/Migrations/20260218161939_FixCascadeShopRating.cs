using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeShopRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Picture_Products_ProductId",
                table: "Picture");

            migrationBuilder.DropForeignKey(
                name: "FK_Picture_ShopRating_ShopRatingId",
                table: "Picture");

            migrationBuilder.DropForeignKey(
                name: "FK_Picture_Shops_ShopId",
                table: "Picture");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopRating_Shops_ShopId",
                table: "ShopRating");

            migrationBuilder.AddForeignKey(
                name: "FK_Picture_Products_ProductId",
                table: "Picture",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Picture_ShopRating_ShopRatingId",
                table: "Picture",
                column: "ShopRatingId",
                principalTable: "ShopRating",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Picture_Shops_ShopId",
                table: "Picture",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopRating_Shops_ShopId",
                table: "ShopRating",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Picture_Products_ProductId",
                table: "Picture");

            migrationBuilder.DropForeignKey(
                name: "FK_Picture_ShopRating_ShopRatingId",
                table: "Picture");

            migrationBuilder.DropForeignKey(
                name: "FK_Picture_Shops_ShopId",
                table: "Picture");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopRating_Shops_ShopId",
                table: "ShopRating");

            migrationBuilder.AddForeignKey(
                name: "FK_Picture_Products_ProductId",
                table: "Picture",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Picture_ShopRating_ShopRatingId",
                table: "Picture",
                column: "ShopRatingId",
                principalTable: "ShopRating",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Picture_Shops_ShopId",
                table: "Picture",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopRating_Shops_ShopId",
                table: "ShopRating",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
