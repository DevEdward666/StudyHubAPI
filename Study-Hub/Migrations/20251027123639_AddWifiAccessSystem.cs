using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Study_Hub.Migrations
{
    /// <inheritdoc />
    public partial class AddWifiAccessSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WifiAccess",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Password = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Note = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Redeemed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WifiAccess", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WifiAccess_ExpiresAtUtc",
                table: "WifiAccess",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_WifiAccess_Password",
                table: "WifiAccess",
                column: "Password",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WifiAccess_Redeemed",
                table: "WifiAccess",
                column: "Redeemed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WifiAccess");
        }
    }
}
