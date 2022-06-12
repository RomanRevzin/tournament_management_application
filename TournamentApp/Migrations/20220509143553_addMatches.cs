using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentApp.Migrations
{
    public partial class addMatches : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchResult",
                table: "Matches");

            migrationBuilder.AddColumn<string>(
                name: "TournamentId",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WinningTeam",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "WinningTeam",
                table: "Matches");

            migrationBuilder.AddColumn<int>(
                name: "MatchResult",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
