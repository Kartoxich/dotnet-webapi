using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEDept.BackendCourse.GroupA1.Api.Template.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("ck_users_status", "status IN ('pending', 'active')");
                });

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_users_email_lower ON users (lower(email));"
            );

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION set_updated_at() RETURNS trigger AS $$
                BEGIN
                    NEW.updated_at := now();
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                CREATE TRIGGER trg_users_updated_at
                    BEFORE UPDATE ON users
                    FOR EACH ROW EXECUTE FUNCTION set_updated_at();
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
