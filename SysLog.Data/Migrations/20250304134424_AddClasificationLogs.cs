using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SysLog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClasificationLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SourceIp",
                table: "Log",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Log",
                newName: "Protocol");

            migrationBuilder.AddColumn<string>(
                name: "Acction",
                table: "Log",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Interface",
                table: "Log",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IpDestiny",
                table: "Log",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IpOut",
                table: "Log",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Acction",
                table: "Log");

            migrationBuilder.DropColumn(
                name: "Interface",
                table: "Log");

            migrationBuilder.DropColumn(
                name: "IpDestiny",
                table: "Log");

            migrationBuilder.DropColumn(
                name: "IpOut",
                table: "Log");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Log",
                newName: "SourceIp");

            migrationBuilder.RenameColumn(
                name: "Protocol",
                table: "Log",
                newName: "Message");
        }
    }
}
