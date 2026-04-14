using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsosEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsosEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuildingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuildingName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventDetails_Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EventDetails_StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventDetails_EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventDetails_Location = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    EventDetails_Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsosEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsosEvents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsosEvents_UserId",
                table: "UsosEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsosEvents_UserId_ExternalKey",
                table: "UsosEvents",
                columns: new[] { "UserId", "ExternalKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsosEvents");
        }
    }
}
