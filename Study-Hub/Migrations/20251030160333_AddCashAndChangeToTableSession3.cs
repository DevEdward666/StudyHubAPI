using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Study_Hub.Migrations
{
    /// <inheritdoc />
    public partial class AddCashAndChangeToTableSession3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "cash",
                table: "table_sessions",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "change",
                table: "table_sessions",
                type: "numeric(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cash",
                table: "table_sessions");

            migrationBuilder.DropColumn(
                name: "change",
                table: "table_sessions");
        }
    }
}
