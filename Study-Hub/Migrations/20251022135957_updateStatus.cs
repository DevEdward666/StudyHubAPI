using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Study_Hub.Migrations
{
    /// <inheritdoc />
    public partial class updateStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_table_sessions_status",
                table: "table_sessions");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "table_sessions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "session_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "table_sessions",
                type: "session_status",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_table_sessions_status",
                table: "table_sessions",
                column: "status");
        }
    }
}
