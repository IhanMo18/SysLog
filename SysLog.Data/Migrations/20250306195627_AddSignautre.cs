using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SysLog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSignautre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Signature",
                table: "Log",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Signature",
                table: "Log");
        }
    }
}
