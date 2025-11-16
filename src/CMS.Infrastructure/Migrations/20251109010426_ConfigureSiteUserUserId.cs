using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureSiteUserUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SiteUsers_SiteId",
                table: "SiteUsers");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "SiteUsers",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_SiteUsers_SiteId_UserId",
                table: "SiteUsers",
                columns: new[] { "SiteId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SiteUsers_SiteId_UserId",
                table: "SiteUsers");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "SiteUsers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450);

            migrationBuilder.CreateIndex(
                name: "IX_SiteUsers_SiteId",
                table: "SiteUsers",
                column: "SiteId");
        }
    }
}
