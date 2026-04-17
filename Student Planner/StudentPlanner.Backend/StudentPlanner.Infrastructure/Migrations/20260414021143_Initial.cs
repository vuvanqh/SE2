using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPlanner.Infrastructure.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });

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

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AcademicEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FacultyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EventDetails_Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                EventDetails_StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                EventDetails_EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                EventDetails_Location = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                EventDetails_Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AcademicEvents", x => x.Id);
                table.ForeignKey(
                    name: "FK_AcademicEvents_Faculties_FacultyId",
                    column: x => x.FacultyId,
                    principalTable: "Faculties",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RefreshTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RefreshTokenExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                RefreshTokenIssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UsosToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                FacultyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUsers_Faculties_FacultyId",
                    column: x => x.FacultyId,
                    principalTable: "Faculties",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

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

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EventRequests",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FacultyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReviewedByAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                EventDetails_Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                EventDetails_StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                EventDetails_EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                EventDetails_Location = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                EventDetails_Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                RequestType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Status = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventRequests", x => x.Id);
                table.ForeignKey(
                    name: "FK_EventRequests_AspNetUsers_ManagerId",
                    column: x => x.ManagerId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_EventRequests_AspNetUsers_ReviewedByAdminId",
                    column: x => x.ReviewedByAdminId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_EventRequests_Faculties_FacultyId",
                    column: x => x.FacultyId,
                    principalTable: "Faculties",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PersonalEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    name: "FK_PersonalEvents_AspNetUsers_ApplicationUserId",
                    column: x => x.ApplicationUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_PersonalEvents_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AcademicEvents_FacultyId",
            table: "AcademicEvents",
            column: "FacultyId");

        migrationBuilder.CreateIndex(
            name: "IX_AcademicEventSubscribers_UserId",
            table: "AcademicEventSubscribers",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true,
            filter: "[NormalizedName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

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
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true,
            filter: "[NormalizedUserName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_EventRequests_FacultyId",
            table: "EventRequests",
            column: "FacultyId");

        migrationBuilder.CreateIndex(
            name: "IX_EventRequests_ManagerId",
            table: "EventRequests",
            column: "ManagerId");

        migrationBuilder.CreateIndex(
            name: "IX_EventRequests_ReviewedByAdminId",
            table: "EventRequests",
            column: "ReviewedByAdminId");

        migrationBuilder.CreateIndex(
            name: "IX_EventRequests_Status",
            table: "EventRequests",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_Faculties_FacultyCode",
            table: "Faculties",
            column: "FacultyCode");

        migrationBuilder.CreateIndex(
            name: "IX_Faculties_FacultyId",
            table: "Faculties",
            column: "FacultyId");

        migrationBuilder.CreateIndex(
            name: "IX_PersonalEvents_ApplicationUserId",
            table: "PersonalEvents",
            column: "ApplicationUserId");

        migrationBuilder.CreateIndex(
            name: "IX_PersonalEvents_UserId",
            table: "PersonalEvents",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AcademicEventSubscribers");

        migrationBuilder.DropTable(
            name: "AspNetRoleClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens");

        migrationBuilder.DropTable(
            name: "EventRequests");

        migrationBuilder.DropTable(
            name: "PersonalEvents");

        migrationBuilder.DropTable(
            name: "AcademicEvents");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "AspNetUsers");

        migrationBuilder.DropTable(
            name: "Faculties");
    }
}
