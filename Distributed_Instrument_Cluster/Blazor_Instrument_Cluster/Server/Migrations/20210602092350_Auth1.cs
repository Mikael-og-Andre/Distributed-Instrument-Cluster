using Microsoft.EntityFrameworkCore.Migrations;

namespace Blazor_Instrument_Cluster.Server.Migrations
{
    public partial class Auth1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "faa9afa0-c382-11eb-8529-0242ac130003", "faa9afa0-c382-11eb-8529-0242ac130003", "User", "User" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "fdf4c924-c382-11eb-8529-0242ac130003", "fdf4c924-c382-11eb-8529-0242ac130003", "Admin", "Admin" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "00665ac4-c383-11eb-8529-0242ac130003", "00665ac4-c383-11eb-8529-0242ac130003", "Control", "Control" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "00665ac4-c383-11eb-8529-0242ac130003");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "faa9afa0-c382-11eb-8529-0242ac130003");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fdf4c924-c382-11eb-8529-0242ac130003");
        }
    }
}
