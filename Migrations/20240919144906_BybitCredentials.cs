using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingTer.Migrations
{
    public partial class BybitCredentials : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StringProperties",
                columns: new[] { "Id", "Description", "Key", "Value" },
                values: new object[] { 4, "Ключ и секрет от апи в формате key:secret", "BybitCredentials", "JK3uhAcX7Zh7Puhtbz:aTTCR8cH8ttm4v4lMypDll9FCGExHUsEVHEF" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StringProperties",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
