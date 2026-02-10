using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmailCaseInsensitive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_Users_Email";

                CREATE UNIQUE INDEX "IX_Users_Email_Lower"
                ON "Users"(LOWER("Email"));
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_Users_Email_Lower";

                CREATE UNIQUE INDEX "IX_Users_Email"
                ON "Users"("Email");
            """);
        }
    }
}
