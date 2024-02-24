using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LytharBackend.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentsNamespace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CdnId",
                table: "Attachment",
                newName: "CdnNamespace");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CdnNamespace",
                table: "Attachment",
                newName: "CdnId");
        }
    }
}
