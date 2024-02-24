using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LytharBackend.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentsId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Attachment",
                newName: "CdnUrl");

            migrationBuilder.AddColumn<string>(
                name: "CdnId",
                table: "Attachment",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CdnId",
                table: "Attachment");

            migrationBuilder.RenameColumn(
                name: "CdnUrl",
                table: "Attachment",
                newName: "Url");
        }
    }
}
