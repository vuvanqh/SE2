using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPlanner.Infrastructure.Migrations;

/// <inheritdoc />
public partial class PolymorphicEvents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "FacultyId1",
            table: "AcademicEvents",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Faculty",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FacultyId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                FacultyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                FacultyCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Faculty", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "User",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RefreshTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RefreshTokenExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                RefreshTokenIssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UsosToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                NotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                FacultyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_User", x => x.Id);
                table.ForeignKey(
                    name: "FK_User_Faculty_FacultyId",
                    column: x => x.FacultyId,
                    principalTable: "Faculty",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_AcademicEvents_FacultyId1",
            table: "AcademicEvents",
            column: "FacultyId1");

        migrationBuilder.CreateIndex(
            name: "IX_User_FacultyId",
            table: "User",
            column: "FacultyId");

        migrationBuilder.AddForeignKey(
            name: "FK_AcademicEvents_Faculty_FacultyId1",
            table: "AcademicEvents",
            column: "FacultyId1",
            principalTable: "Faculty",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AcademicEvents_Faculty_FacultyId1",
            table: "AcademicEvents");

        migrationBuilder.DropTable(
            name: "User");

        migrationBuilder.DropTable(
            name: "Faculty");

        migrationBuilder.DropIndex(
            name: "IX_AcademicEvents_FacultyId1",
            table: "AcademicEvents");

        migrationBuilder.DropColumn(
            name: "FacultyId1",
            table: "AcademicEvents");
    }
}
