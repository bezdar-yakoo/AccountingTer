using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingTer.Migrations
{
    public partial class Url : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StringProperties",
                keyColumn: "Id",
                keyValue: 2,
                column: "Value",
                value: "23");

            migrationBuilder.InsertData(
                table: "StringProperties",
                columns: new[] { "Id", "Description", "Key", "Value" },
                values: new object[] { 5, "Ссылка на веб интерфейс", "Url", "https://google.com" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StringProperties",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "StringProperties",
                keyColumn: "Id",
                keyValue: 2,
                column: "Value",
                value: "13");
        }
    }
}
