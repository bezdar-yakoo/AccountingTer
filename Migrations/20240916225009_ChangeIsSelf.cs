using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingTer.Migrations
{
    public partial class ChangeIsSelf : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSelf",
                table: "BalanceEvents",
                newName: "OwnerBalanceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnerBalanceId",
                table: "BalanceEvents",
                newName: "IsSelf");
        }
    }
}
