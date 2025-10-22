using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Study_Hub.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:session_status", "active,completed")
                .Annotation("Npgsql:Enum:session_status.session_status", "active,completed");

            migrationBuilder.CreateTable(
                name: "premise_qr_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    validity_hours = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_premise_qr_codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    image = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    phone_verified = table.Column<bool>(type: "boolean", nullable: false),
                    is_anonymous = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "admin_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_admin_users_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auth_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    provider_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    provider_account_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    secret = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_auth_accounts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "credit_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    cost = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    payment_method = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    transaction_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credit_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_credit_transactions_users_approved_by",
                        column: x => x.approved_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_credit_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "premise_activation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    activation_code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_premise_activation", x => x.id);
                    table.ForeignKey(
                        name: "FK_premise_activation_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "study_tables",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    table_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    qr_code = table.Column<string>(type: "text", nullable: false),
                    qr_code_image = table.Column<string>(type: "text", nullable: true),
                    is_occupied = table.Column<bool>(type: "boolean", nullable: false),
                    current_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    hourly_rate = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_tables", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_tables_users_current_user_id",
                        column: x => x.current_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_credits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    total_purchased = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    total_spent = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_credits", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_credits_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "table_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    credits_used = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    status = table.Column<string>(type: "session_status", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_table_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_table_sessions_study_tables_table_id",
                        column: x => x.table_id,
                        principalTable: "study_tables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_table_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_user_id",
                table: "admin_users",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_auth_accounts_provider_provider_account_id",
                table: "auth_accounts",
                columns: new[] { "provider", "provider_account_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_auth_accounts_user_id",
                table: "auth_accounts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_credit_transactions_approved_by",
                table: "credit_transactions",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_credit_transactions_status",
                table: "credit_transactions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_credit_transactions_user_id",
                table: "credit_transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_premise_activation_expires_at",
                table: "premise_activation",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_premise_activation_is_active",
                table: "premise_activation",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_premise_activation_user_id",
                table: "premise_activation",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_premise_qr_codes_code",
                table: "premise_qr_codes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_study_tables_current_user_id",
                table: "study_tables",
                column: "current_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_tables_qr_code",
                table: "study_tables",
                column: "qr_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_study_tables_table_number",
                table: "study_tables",
                column: "table_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_table_sessions_status",
                table: "table_sessions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_table_sessions_table_id",
                table: "table_sessions",
                column: "table_id");

            migrationBuilder.CreateIndex(
                name: "IX_table_sessions_user_id",
                table: "table_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_credits_user_id",
                table: "user_credits",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_users");

            migrationBuilder.DropTable(
                name: "auth_accounts");

            migrationBuilder.DropTable(
                name: "credit_transactions");

            migrationBuilder.DropTable(
                name: "premise_activation");

            migrationBuilder.DropTable(
                name: "premise_qr_codes");

            migrationBuilder.DropTable(
                name: "table_sessions");

            migrationBuilder.DropTable(
                name: "user_credits");

            migrationBuilder.DropTable(
                name: "study_tables");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
