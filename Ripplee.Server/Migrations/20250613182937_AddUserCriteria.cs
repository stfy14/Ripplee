using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ripplee.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MyCity",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MyGender",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MyCity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MyGender",
                table: "Users");
        }
    }
}
