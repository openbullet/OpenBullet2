using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBullet2.Core.Migrations
{
    public partial class ProxyGroupDeleteRelationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proxies_ProxyGroups_GroupId",
                table: "Proxies");

            migrationBuilder.DropForeignKey(
                name: "FK_ProxyGroups_Guests_OwnerId",
                table: "ProxyGroups");

            migrationBuilder.AddForeignKey(
                name: "FK_Proxies_ProxyGroups_GroupId",
                table: "Proxies",
                column: "GroupId",
                principalTable: "ProxyGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProxyGroups_Guests_OwnerId",
                table: "ProxyGroups",
                column: "OwnerId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proxies_ProxyGroups_GroupId",
                table: "Proxies");

            migrationBuilder.DropForeignKey(
                name: "FK_ProxyGroups_Guests_OwnerId",
                table: "ProxyGroups");

            migrationBuilder.AddForeignKey(
                name: "FK_Proxies_ProxyGroups_GroupId",
                table: "Proxies",
                column: "GroupId",
                principalTable: "ProxyGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProxyGroups_Guests_OwnerId",
                table: "ProxyGroups",
                column: "OwnerId",
                principalTable: "Guests",
                principalColumn: "Id");
        }
    }
}
