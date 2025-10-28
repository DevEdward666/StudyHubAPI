using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Study_Hub.Migrations
{
    /// <inheritdoc />
    public partial class AddPromoSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WifiAccess",
                table: "WifiAccess");

            migrationBuilder.RenameTable(
                name: "WifiAccess",
                newName: "WifiAccesses");

            migrationBuilder.RenameIndex(
                name: "IX_WifiAccess_Redeemed",
                table: "WifiAccesses",
                newName: "IX_WifiAccesses_Redeemed");

            migrationBuilder.RenameIndex(
                name: "IX_WifiAccess_Password",
                table: "WifiAccesses",
                newName: "IX_WifiAccesses_Password");

            migrationBuilder.RenameIndex(
                name: "IX_WifiAccess_ExpiresAtUtc",
                table: "WifiAccesses",
                newName: "IX_WifiAccesses_ExpiresAtUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WifiAccesses",
                table: "WifiAccesses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "promos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    percentage_bonus = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    fixed_bonus_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    buy_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    get_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    min_purchase_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    max_discount_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    usage_limit = table.Column<int>(type: "integer", nullable: true),
                    usage_per_user = table.Column<int>(type: "integer", nullable: true),
                    current_usage_count = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promos", x => x.id);
                    table.ForeignKey(
                        name: "FK_promos_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promo_usages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    promo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    purchase_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    bonus_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promo_usages", x => x.id);
                    table.ForeignKey(
                        name: "FK_promo_usages_credit_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "credit_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_promo_usages_promos_promo_id",
                        column: x => x.promo_id,
                        principalTable: "promos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_promo_usages_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_promo_usages_promo_id",
                table: "promo_usages",
                column: "promo_id");

            migrationBuilder.CreateIndex(
                name: "IX_promo_usages_transaction_id",
                table: "promo_usages",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_promo_usages_used_at",
                table: "promo_usages",
                column: "used_at");

            migrationBuilder.CreateIndex(
                name: "IX_promo_usages_user_id",
                table: "promo_usages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_promos_code",
                table: "promos",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_promos_created_by",
                table: "promos",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_promos_is_deleted",
                table: "promos",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_promos_start_date_end_date",
                table: "promos",
                columns: new[] { "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "IX_promos_status",
                table: "promos",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_promos_type",
                table: "promos",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "promo_usages");

            migrationBuilder.DropTable(
                name: "promos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WifiAccesses",
                table: "WifiAccesses");

            migrationBuilder.RenameTable(
                name: "WifiAccesses",
                newName: "WifiAccess");

            migrationBuilder.RenameIndex(
                name: "IX_WifiAccesses_Redeemed",
                table: "WifiAccess",
                newName: "IX_WifiAccess_Redeemed");

            migrationBuilder.RenameIndex(
                name: "IX_WifiAccesses_Password",
                table: "WifiAccess",
                newName: "IX_WifiAccess_Password");

            migrationBuilder.RenameIndex(
                name: "IX_WifiAccesses_ExpiresAtUtc",
                table: "WifiAccess",
                newName: "IX_WifiAccess_ExpiresAtUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WifiAccess",
                table: "WifiAccess",
                column: "Id");
        }
    }
}
