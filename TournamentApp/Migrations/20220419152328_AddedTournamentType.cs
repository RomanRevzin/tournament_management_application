using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentApp.Migrations
{
    public partial class AddedTournamentType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TournamentName",
                table: "Tournaments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "TeamSize",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0) ;

            migrationBuilder.CreateTable(
                name: "TournamentTypes",
                columns: table => new
                {
                    TypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentTypes", x => x.TypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_TypeId",
                table: "Tournaments",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_TournamentTypes_TypeId",
                table: "Tournaments",
                column: "TypeId",
                principalTable: "TournamentTypes",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_TournamentTypes_TypeId",
                table: "Tournaments");

            migrationBuilder.DropTable(
                name: "TournamentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_TypeId",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "TeamSize",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Tournaments");

            migrationBuilder.AlterColumn<string>(
                name: "TournamentName",
                table: "Tournaments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
