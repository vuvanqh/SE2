using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPlanner.Infrastructure.Migrations;

/// <inheritdoc />
public partial class RefactorAcademicEventSubscribersToJoinTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SubscriberIds",
            table: "AcademicEvents");

        migrationBuilder.CreateTable(
            name: "AcademicEventSubscribers",
            columns: table => new
            {
                AcademicEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AcademicEventSubscribers", x => new { x.AcademicEventId, x.UserId });
                table.ForeignKey(
                    name: "FK_AcademicEventSubscribers_AcademicEvents_AcademicEventId",
                    column: x => x.AcademicEventId,
                    principalTable: "AcademicEvents",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AcademicEventSubscribers_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_AcademicEventSubscribers_UserId",
            table: "AcademicEventSubscribers",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AcademicEventSubscribers");

        migrationBuilder.AddColumn<string>(
            name: "SubscriberIds",
            table: "AcademicEvents",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }
}
