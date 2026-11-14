using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEDept.BackendCourse.GroupA1.Api.Template.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "registration_challenges",
                columns: table => new
                {
                    registration_challenge_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    secret_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    method = table.Column<string>(type: "text", nullable: false, defaultValue: "otp"),
                    attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    consumed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registration_challenges", x => x.registration_challenge_id);
                    table.CheckConstraint("ck_challenges_method", "method IN ('otp', 'magic_link')");
                    table.ForeignKey(
                        name: "FK_registration_challenges_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_registration_challenges_user_id",
                table: "registration_challenges",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registration_challenges");
        }
    }
}
