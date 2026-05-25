using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBullet2.Core.Migrations;

/// <inheritdoc />
public partial class AddJobLastRunOutcome : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "LastRunOutcome",
            table: "Jobs",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastRunOutcome",
            table: "Jobs");
    }
}
