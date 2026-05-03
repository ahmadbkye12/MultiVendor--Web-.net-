using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VendorDashboardSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BannerUrl",
                table: "VendorStores",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "VendorStores",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "VendorStores",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "VendorStores",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultCommissionPercent",
                table: "Vendors",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionAmount",
                table: "OrderItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPercent",
                table: "OrderItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LineTotal",
                table: "OrderItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "VendorFulfillmentStatus",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "VendorNetAmount",
                table: "OrderItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "VendorStoreId",
                table: "OrderItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_VendorStoreId",
                table: "OrderItems",
                column: "VendorStoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_VendorStores_VendorStoreId",
                table: "OrderItems",
                column: "VendorStoreId",
                principalTable: "VendorStores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_VendorStores_VendorStoreId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_VendorStoreId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "BannerUrl",
                table: "VendorStores");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "VendorStores");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "VendorStores");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "VendorStores");

            migrationBuilder.DropColumn(
                name: "DefaultCommissionPercent",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CommissionAmount",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CommissionPercent",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "LineTotal",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VendorFulfillmentStatus",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VendorNetAmount",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VendorStoreId",
                table: "OrderItems");
        }
    }
}
