using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEDept.BackendCourse.GroupA1.Api.Template.Migrations
{
    /// <inheritdoc />
    public partial class AddOneLiveChallengePerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_registration_challenges_user_id",
                table: "registration_challenges");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "users",
                type: "text",
                nullable: false,
                defaultValueSql: "'pending'",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "pending");

            migrationBuilder.AlterColumn<string>(
                name: "method",
                table: "registration_challenges",
                type: "text",
                nullable: false,
                defaultValueSql: "'otp'",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "otp");

            migrationBuilder.CreateIndex(
                name: "IX_registration_challenges_user_id",
                table: "registration_challenges",
                column: "user_id",
                unique: true,
                filter: "consumed_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_registration_challenges_user_id",
                table: "registration_challenges");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "pending",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValueSql: "'pending'");

            migrationBuilder.AlterColumn<string>(
                name: "method",
                table: "registration_challenges",
                type: "text",
                nullable: false,
                defaultValue: "otp",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValueSql: "'otp'");

            migrationBuilder.CreateIndex(
                name: "IX_registration_challenges_user_id",
                table: "registration_challenges",
                column: "user_id");
        }
    }
}
