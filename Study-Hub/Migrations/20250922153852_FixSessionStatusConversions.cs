using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Study_Hub.Migrations
{
    /// <inheritdoc />
    public partial class FixSessionStatusConversions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:session_status", "active,completed")
                .Annotation("Npgsql:Enum:session_status.session_status", "active,completed")
                .OldAnnotation("Npgsql:Enum:session_status.session_status", "active,completed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:session_status.session_status", "active,completed")
                .OldAnnotation("Npgsql:Enum:session_status", "active,completed")
                .OldAnnotation("Npgsql:Enum:session_status.session_status", "active,completed");
        }
    }
}
