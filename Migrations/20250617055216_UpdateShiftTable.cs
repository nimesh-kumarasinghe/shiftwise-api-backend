using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftWiseAI.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShiftTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DaysCount",
                table: "Shifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SkipWeekends",
                table: "Shifts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysCount",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "SkipWeekends",
                table: "Shifts");
        }
    }
}
