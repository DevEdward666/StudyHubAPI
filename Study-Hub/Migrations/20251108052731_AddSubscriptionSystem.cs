using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Study_Hub.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "hours_consumed",
                table: "table_sessions",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_subscription_based",
                table: "table_sessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "subscription_id",
                table: "table_sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "subscription_packages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    package_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    duration_value = table.Column<int>(type: "integer", nullable: false),
                    total_hours = table.Column<decimal>(type: "numeric", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_packages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    package_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_hours = table.Column<decimal>(type: "numeric", nullable: false),
                    remaining_hours = table.Column<decimal>(type: "numeric", nullable: false),
                    hours_used = table.Column<decimal>(type: "numeric", nullable: false),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    purchase_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    transaction_reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_subscriptions_subscription_packages_package_id",
                        column: x => x.package_id,
                        principalTable: "subscription_packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_subscriptions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_table_sessions_subscription_id",
                table: "table_sessions",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_subscriptions_package_id",
                table: "user_subscriptions",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_subscriptions_user_id",
                table: "user_subscriptions",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_table_sessions_user_subscriptions_subscription_id",
                table: "table_sessions",
                column: "subscription_id",
                principalTable: "user_subscriptions",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_table_sessions_user_subscriptions_subscription_id",
                table: "table_sessions");

            migrationBuilder.DropTable(
                name: "user_subscriptions");

            migrationBuilder.DropTable(
                name: "subscription_packages");

            migrationBuilder.DropIndex(
                name: "IX_table_sessions_subscription_id",
                table: "table_sessions");

            migrationBuilder.DropColumn(
                name: "hours_consumed",
                table: "table_sessions");

            migrationBuilder.DropColumn(
                name: "is_subscription_based",
                table: "table_sessions");

            migrationBuilder.DropColumn(
                name: "subscription_id",
                table: "table_sessions");
        }
    }
}
