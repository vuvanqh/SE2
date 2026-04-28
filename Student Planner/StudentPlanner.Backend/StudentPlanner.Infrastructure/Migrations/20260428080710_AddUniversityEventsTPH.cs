using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPlanner.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddUniversityEventsTPH : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Guid>(
            name: "FacultyId",
            table: "EventRequests",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier");

        migrationBuilder.AlterColumn<Guid>(
            name: "FacultyId",
            table: "AcademicEvents",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier");

        migrationBuilder.AddColumn<string>(
            name: "EventType",
            table: "AcademicEvents",
            type: "nvarchar(13)",
            maxLength: 13,
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "EventType",
            table: "AcademicEvents");

        migrationBuilder.AlterColumn<Guid>(
            name: "FacultyId",
            table: "EventRequests",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: Guid.Empty,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "FacultyId",
            table: "AcademicEvents",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: Guid.Empty,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);
    }
}
