using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPlanner.Infrastructure.Migrations;

/// <inheritdoc />
public partial class PersonalEvents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PersonalEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EventDetails_Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                EventDetails_StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                EventDetails_EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                EventDetails_Location = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                EventDetails_Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PersonalEvents", x => x.Id);
                table.ForeignKey(
                    name: "FK_PersonalEvents_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PersonalEvents_UserId",
            table: "PersonalEvents",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PersonalEvents");
    }
}
