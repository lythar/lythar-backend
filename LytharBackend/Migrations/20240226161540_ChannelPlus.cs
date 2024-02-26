using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LytharBackend.Migrations
{
    /// <inheritdoc />
    public partial class ChannelPlus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChannelId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "Channels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconId",
                table: "Channels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "Channels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDirectMessages",
                table: "Channels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Channels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ChannelId",
                table: "Users",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_CreatorId",
                table: "Channels",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Users_CreatorId",
                table: "Channels",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Channels_ChannelId",
                table: "Users",
                column: "ChannelId",
                principalTable: "Channels",
                principalColumn: "ChannelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Users_CreatorId",
                table: "Channels");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Channels_ChannelId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ChannelId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Channels_CreatorId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "IconId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "IsDirectMessages",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Channels");
        }
    }
}
