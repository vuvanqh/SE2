using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPlanner.Infrastructure.Migrations;

/// <inheritdoc />
public partial class Usos : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "ApplicationUserId",
            table: "PersonalEvents",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "FacultyId",
            table: "AspNetUsers",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UsosToken",
            table: "AspNetUsers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Faculties",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FacultyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                FacultyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                FacultyCode = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Faculties", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PersonalEvents_ApplicationUserId",
            table: "PersonalEvents",
            column: "ApplicationUserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUsers_Email",
            table: "AspNetUsers",
            column: "Email",
            unique: true,
            filter: "[Email] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUsers_FacultyId",
            table: "AspNetUsers",
            column: "FacultyId");

        migrationBuilder.CreateIndex(
            name: "IX_Faculties_FacultyCode",
            table: "Faculties",
            column: "FacultyCode");

        migrationBuilder.CreateIndex(
            name: "IX_Faculties_FacultyId",
            table: "Faculties",
            column: "FacultyId");

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUsers_Faculties_FacultyId",
            table: "AspNetUsers",
            column: "FacultyId",
            principalTable: "Faculties",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_PersonalEvents_AspNetUsers_ApplicationUserId",
            table: "PersonalEvents",
            column: "ApplicationUserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUsers_Faculties_FacultyId",
            table: "AspNetUsers");

        migrationBuilder.DropForeignKey(
            name: "FK_PersonalEvents_AspNetUsers_ApplicationUserId",
            table: "PersonalEvents");

        migrationBuilder.DropTable(
            name: "Faculties");

        migrationBuilder.DropIndex(
            name: "IX_PersonalEvents_ApplicationUserId",
            table: "PersonalEvents");

        migrationBuilder.DropIndex(
            name: "IX_AspNetUsers_Email",
            table: "AspNetUsers");

        migrationBuilder.DropIndex(
            name: "IX_AspNetUsers_FacultyId",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "ApplicationUserId",
            table: "PersonalEvents");

        migrationBuilder.DropColumn(
            name: "FacultyId",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "UsosToken",
            table: "AspNetUsers");
    }
}
