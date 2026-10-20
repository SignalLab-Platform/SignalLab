using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignalLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredFileOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "stored_files",
                type: "uuid",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_OwnerUserId",
                table: "stored_files",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_stored_files_users_OwnerUserId",
                table: "stored_files",
                column: "OwnerUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stored_files_users_OwnerUserId",
                table: "stored_files");

            migrationBuilder.DropIndex(
                name: "IX_stored_files_OwnerUserId",
                table: "stored_files");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "stored_files");
        }
    }
}
