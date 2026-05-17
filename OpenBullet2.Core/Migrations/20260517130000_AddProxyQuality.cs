using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBullet2.Core.Migrations;

public partial class AddProxyQuality : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Quality",
            table: "Proxies",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Quality",
            table: "Proxies");
    }
}
