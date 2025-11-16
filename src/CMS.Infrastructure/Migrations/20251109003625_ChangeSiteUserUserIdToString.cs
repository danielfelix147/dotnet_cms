using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSiteUserUserIdToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SiteUsers_Users_UserId",
                table: "SiteUsers");

            migrationBuilder.DropIndex(
                name: "IX_SiteUsers_UserId",
                table: "SiteUsers");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "SiteUsers",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "SiteUsers",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_SiteUsers_UserId",
                table: "SiteUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SiteUsers_Users_UserId",
                table: "SiteUsers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
