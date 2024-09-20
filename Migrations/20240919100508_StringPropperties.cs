using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingTer.Migrations
{
    public partial class StringPropperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StringProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StringProperties", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "StringProperties",
                columns: new[] { "Id", "Key", "Value" },
                values: new object[] { 1, "IdsForBackupDataBase", "475031431" });

            migrationBuilder.InsertData(
                table: "StringProperties",
                columns: new[] { "Id", "Key", "Value" },
                values: new object[] { 2, "DailyStatisticHour", "13" });

            migrationBuilder.InsertData(
                table: "StringProperties",
                columns: new[] { "Id", "Key", "Value" },
                values: new object[] { 3, "ChatsForStatistic", "475031431" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StringProperties");
        }
    }
}
