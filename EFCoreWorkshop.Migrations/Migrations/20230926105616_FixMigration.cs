using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreWorkshop.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class FixMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Worker_WorkerEntityWorkerId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_WorkerEntityWorkerId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "WorkerEntityWorkerId",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_WorkerId",
                table: "Tasks",
                column: "WorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Worker_WorkerId",
                table: "Tasks",
                column: "WorkerId",
                principalTable: "Worker",
                principalColumn: "WorkerId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Worker_WorkerId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_WorkerId",
                table: "Tasks");

            migrationBuilder.AddColumn<Guid>(
                name: "WorkerEntityWorkerId",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_WorkerEntityWorkerId",
                table: "Tasks",
                column: "WorkerEntityWorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Worker_WorkerEntityWorkerId",
                table: "Tasks",
                column: "WorkerEntityWorkerId",
                principalTable: "Worker",
                principalColumn: "WorkerId");
        }
    }
}
