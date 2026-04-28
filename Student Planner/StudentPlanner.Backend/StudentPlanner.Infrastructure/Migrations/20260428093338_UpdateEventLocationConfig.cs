using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPlanner.Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdateEventLocationConfig : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "EventDetails_Location",
            table: "PersonalEvents",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(70)",
            oldMaxLength: 70,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "EventDetails_Location",
            table: "EventRequests",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(70)",
            oldMaxLength: 70,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "EventDetails_Location",
            table: "AcademicEvents",
            type: "nvarchar(70)",
            maxLength: 70,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(70)",
            oldMaxLength: 70,
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "EventDetails_Location",
            table: "PersonalEvents",
            type: "nvarchar(70)",
            maxLength: 70,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "EventDetails_Location",
            table: "EventRequests",
            type: "nvarchar(70)",
            maxLength: 70,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "EventDetails_Location",
            table: "AcademicEvents",
            type: "nvarchar(70)",
            maxLength: 70,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(70)",
            oldMaxLength: 70);
    }
}
