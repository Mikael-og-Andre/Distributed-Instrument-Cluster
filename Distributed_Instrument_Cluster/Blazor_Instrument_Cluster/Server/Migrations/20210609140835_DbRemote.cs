using Microsoft.EntityFrameworkCore.Migrations;

namespace Blazor_Instrument_Cluster.Server.Migrations
{
    public partial class DbRemote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ipAddress",
                table: "devices",
                newName: "ip");

            migrationBuilder.AddColumn<int>(
                name: "crestronPort",
                table: "devices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "hasCrestron",
                table: "devices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "videoBasePort",
                table: "devices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "videoDeviceNumber",
                table: "devices",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "crestronPort",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "hasCrestron",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "videoBasePort",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "videoDeviceNumber",
                table: "devices");

            migrationBuilder.RenameColumn(
                name: "ip",
                table: "devices",
                newName: "ipAddress");
        }
    }
}
