using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LytharBackend.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Messages_AttachedToMessageId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "AttachedToId",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "AttachedToMessageId",
                table: "Attachments",
                newName: "MessageId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_AttachedToMessageId",
                table: "Attachments",
                newName: "IX_Attachments_MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Messages_MessageId",
                table: "Attachments",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Messages_MessageId",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "Attachments",
                newName: "AttachedToMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_MessageId",
                table: "Attachments",
                newName: "IX_Attachments_AttachedToMessageId");

            migrationBuilder.AddColumn<int>(
                name: "AttachedToId",
                table: "Attachments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Messages_AttachedToMessageId",
                table: "Attachments",
                column: "AttachedToMessageId",
                principalTable: "Messages",
                principalColumn: "MessageId");
        }
    }
}
