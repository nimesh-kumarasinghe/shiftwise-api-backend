using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftWiseAI.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddWeekylyHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxWeeklyHours",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxWeeklyHours",
                table: "Employees");
        }
    }
}
