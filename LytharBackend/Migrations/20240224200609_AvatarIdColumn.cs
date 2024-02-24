using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LytharBackend.Migrations
{
    /// <inheritdoc />
    public partial class AvatarIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarId",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "Users");
        }
    }
}
