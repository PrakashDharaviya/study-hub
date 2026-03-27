using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyHub.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsAndActivityFeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TopicTags",
                table: "StudyGroups",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateTable(
                name: "ActivityFeeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityFeeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityFeeds_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityFeeds_StudyGroups_StudyGroupId",
                        column: x => x.StudyGroupId,
                        principalTable: "StudyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupTags",
                columns: table => new
                {
                    StudyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupTags", x => new { x.StudyGroupId, x.TagId });
                    table.ForeignKey(
                        name: "FK_GroupTags_StudyGroups_StudyGroupId",
                        column: x => x.StudyGroupId,
                        principalTable: "StudyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityFeeds_StudyGroupId",
                table: "ActivityFeeds",
                column: "StudyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityFeeds_UserId",
                table: "ActivityFeeds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTags_TagId",
                table: "GroupTags",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityFeeds");

            migrationBuilder.DropTable(
                name: "GroupTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.AlterColumn<string>(
                name: "TopicTags",
                table: "StudyGroups",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
