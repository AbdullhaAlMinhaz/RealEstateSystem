using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstateSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyCommissionRatePercent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommissionRatePercent",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommissionRatePercent",
                table: "Properties");
        }
    }
}
