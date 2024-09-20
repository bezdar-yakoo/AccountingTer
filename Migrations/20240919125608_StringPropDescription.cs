using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingTer.Migrations
{
    public partial class StringPropDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StringProperties",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Balance",
                table: "Owners",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<double>(
                name: "Value",
                table: "BalanceEvents",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.UpdateData(
                table: "StringProperties",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "ID чатов, в которые бот будет присылать дамп базы данных");

            migrationBuilder.UpdateData(
                table: "StringProperties",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "час, когда надо присылать статистику и дамп базы данных");

            migrationBuilder.UpdateData(
                table: "StringProperties",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "ID чатов, в которые бот будет писать статистику в конце дня");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "StringProperties");

            migrationBuilder.AlterColumn<int>(
                name: "Balance",
                table: "Owners",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<int>(
                name: "Value",
                table: "BalanceEvents",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");
        }
    }
}
