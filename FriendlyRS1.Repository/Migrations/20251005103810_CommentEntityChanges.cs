using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FriendlyRS1.Repository.Migrations
{
    /// <inheritdoc />
    public partial class CommentEntityChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PostId1",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PostId1",
                table: "Comments",
                column: "PostId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Post_PostId1",
                table: "Comments",
                column: "PostId1",
                principalTable: "Post",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Post_PostId1",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_PostId1",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "PostId1",
                table: "Comments");
        }
    }
}
