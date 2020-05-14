using Microsoft.EntityFrameworkCore.Migrations;

namespace WebStore.UI.Data.Migrations
{
    public partial class ChangePriceFormatInMenuItemFromDecimalToDouble : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "MenuItem",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 2)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "MenuItem",
                type: "decimal(18, 2)",
                nullable: false,
                oldClrType: typeof(double));
        }
    }
}
