using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Study_Hub.Migrations
{
    /// <inheritdoc />
    public partial class AddRateToTableSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "rate_id",
                table: "table_sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_table_sessions_rate_id",
                table: "table_sessions",
                column: "rate_id");

            migrationBuilder.AddForeignKey(
                name: "FK_table_sessions_rates_rate_id",
                table: "table_sessions",
                column: "rate_id",
                principalTable: "rates",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_table_sessions_rates_rate_id",
                table: "table_sessions");

            migrationBuilder.DropIndex(
                name: "IX_table_sessions_rate_id",
                table: "table_sessions");

            migrationBuilder.DropColumn(
                name: "rate_id",
                table: "table_sessions");
        }
    }
}

