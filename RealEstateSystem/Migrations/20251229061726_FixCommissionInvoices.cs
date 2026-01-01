//using Microsoft.EntityFrameworkCore.Migrations;

//#nullable disable

//namespace RealEstateSystem.Migrations
//{
//    /// <inheritdoc />
//    public partial class FixCommissionInvoices : Migration
//    {
//        /// <inheritdoc />
//        protected override void Up(MigrationBuilder migrationBuilder)
//        {
//            migrationBuilder.AddColumn<string>(
//                name: "GatewayStatus",
//                table: "CommissionInvoices",
//                type: "nvarchar(max)",
//                nullable: false,
//                defaultValue: "");

//            migrationBuilder.AddColumn<string>(
//                name: "GatewayTranId",
//                table: "CommissionInvoices",
//                type: "nvarchar(max)",
//                nullable: false,
//                defaultValue: "");

//            migrationBuilder.AddColumn<string>(
//                name: "GatewayValId",
//                table: "CommissionInvoices",
//                type: "nvarchar(max)",
//                nullable: false,
//                defaultValue: "");
//        }

//        /// <inheritdoc />
//        protected override void Down(MigrationBuilder migrationBuilder)
//        {
//            migrationBuilder.DropColumn(
//                name: "GatewayStatus",
//                table: "CommissionInvoices");

//            migrationBuilder.DropColumn(
//                name: "GatewayTranId",
//                table: "CommissionInvoices");

//            migrationBuilder.DropColumn(
//                name: "GatewayValId",
//                table: "CommissionInvoices");
//        }
//    }
//}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstateSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixCommissionInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Add GatewayStatus only if it does not exist
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.CommissionInvoices', 'GatewayStatus') IS NULL
BEGIN
    ALTER TABLE dbo.CommissionInvoices
    ADD GatewayStatus nvarchar(max) NOT NULL CONSTRAINT DF_CommissionInvoices_GatewayStatus DEFAULT N'';
END
");

            // ✅ Add GatewayTranId only if it does not exist
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.CommissionInvoices', 'GatewayTranId') IS NULL
BEGIN
    ALTER TABLE dbo.CommissionInvoices
    ADD GatewayTranId nvarchar(max) NOT NULL CONSTRAINT DF_CommissionInvoices_GatewayTranId DEFAULT N'';
END
");

            // ✅ Add GatewayValId only if it does not exist
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.CommissionInvoices', 'GatewayValId') IS NULL
BEGIN
    ALTER TABLE dbo.CommissionInvoices
    ADD GatewayValId nvarchar(max) NOT NULL CONSTRAINT DF_CommissionInvoices_GatewayValId DEFAULT N'';
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ Drop GatewayStatus safely (drop default constraint if exists)
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.CommissionInvoices', 'GatewayStatus') IS NOT NULL
BEGIN
    DECLARE @df1 NVARCHAR(128);
    SELECT @df1 = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'CommissionInvoices' AND c.name = 'GatewayStatus';

    IF @df1 IS NOT NULL
        EXEC('ALTER TABLE dbo.CommissionInvoices DROP CONSTRAINT ' + @df1);

    ALTER TABLE dbo.CommissionInvoices DROP COLUMN GatewayStatus;
END
");

            // ✅ Drop GatewayTranId safely
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.CommissionInvoices', 'GatewayTranId') IS NOT NULL
BEGIN
    DECLARE @df2 NVARCHAR(128);
    SELECT @df2 = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'CommissionInvoices' AND c.name = 'GatewayTranId';

    IF @df2 IS NOT NULL
        EXEC('ALTER TABLE dbo.CommissionInvoices DROP CONSTRAINT ' + @df2);

    ALTER TABLE dbo.CommissionInvoices DROP COLUMN GatewayTranId;
END
");

            // ✅ Drop GatewayValId safely
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.CommissionInvoices', 'GatewayValId') IS NOT NULL
BEGIN
    DECLARE @df3 NVARCHAR(128);
    SELECT @df3 = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'CommissionInvoices' AND c.name = 'GatewayValId';

    IF @df3 IS NOT NULL
        EXEC('ALTER TABLE dbo.CommissionInvoices DROP CONSTRAINT ' + @df3);

    ALTER TABLE dbo.CommissionInvoices DROP COLUMN GatewayValId;
END
");
        }
    }
}

