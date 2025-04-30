using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace triliza.Migrations
{
    /// <inheritdoc />
    public partial class Initial1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_PreSession_MatchId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_MatchId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_PreSession_UserId",
                table: "PreSession");

            migrationBuilder.DropColumn(
                name: "MatchId",
                table: "Sessions");

            migrationBuilder.AddColumn<bool>(
                name: "IsFull",
                table: "Sessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PreSession_UserId",
                table: "PreSession",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PreSession_UserId",
                table: "PreSession");

            migrationBuilder.DropColumn(
                name: "IsFull",
                table: "Sessions");

            migrationBuilder.AddColumn<int>(
                name: "MatchId",
                table: "Sessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_MatchId",
                table: "Sessions",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PreSession_UserId",
                table: "PreSession",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_PreSession_MatchId",
                table: "Sessions",
                column: "MatchId",
                principalTable: "PreSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
