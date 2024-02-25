using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LytharBackend.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_Messages_MessageId",
                table: "Attachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attachment",
                table: "Attachment");

            migrationBuilder.RenameTable(
                name: "Attachment",
                newName: "Attachments");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "Attachments",
                newName: "AttachedToMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachment_MessageId",
                table: "Attachments",
                newName: "IX_Attachments_AttachedToMessageId");

            migrationBuilder.AddColumn<int>(
                name: "AttachedToId",
                table: "Attachments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attachments",
                table: "Attachments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Messages_AttachedToMessageId",
                table: "Attachments",
                column: "AttachedToMessageId",
                principalTable: "Messages",
                principalColumn: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Messages_AttachedToMessageId",
                table: "Attachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attachments",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "AttachedToId",
                table: "Attachments");

            migrationBuilder.RenameTable(
                name: "Attachments",
                newName: "Attachment");

            migrationBuilder.RenameColumn(
                name: "AttachedToMessageId",
                table: "Attachment",
                newName: "MessageId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_AttachedToMessageId",
                table: "Attachment",
                newName: "IX_Attachment_MessageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attachment",
                table: "Attachment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_Messages_MessageId",
                table: "Attachment",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "MessageId");
        }
    }
}
