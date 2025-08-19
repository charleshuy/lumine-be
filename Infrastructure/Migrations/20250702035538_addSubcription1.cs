using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addSubcription1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionTierId1",
                table: "UserSubscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_SubscriptionTierId1",
                table: "UserSubscriptions",
                column: "SubscriptionTierId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_SubscriptionTiers_SubscriptionTierId1",
                table: "UserSubscriptions",
                column: "SubscriptionTierId1",
                principalTable: "SubscriptionTiers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_SubscriptionTiers_SubscriptionTierId1",
                table: "UserSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_SubscriptionTierId1",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "SubscriptionTierId1",
                table: "UserSubscriptions");
        }
    }
}
