using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace triliza.Migrations
{
    /// <inheritdoc />
    public partial class Initial5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PreSession_UserId",
                table: "PreSession");

            migrationBuilder.CreateIndex(
                name: "IX_PreSession_UserId",
                table: "PreSession",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PreSession_UserId",
                table: "PreSession");

            migrationBuilder.CreateIndex(
                name: "IX_PreSession_UserId",
                table: "PreSession",
                column: "UserId",
                unique: true);
        }
    }
}
