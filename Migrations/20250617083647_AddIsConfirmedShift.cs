using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftWiseAI.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddIsConfirmedShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "ShiftAssignments");

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "Shifts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "Shifts");

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "ShiftAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
