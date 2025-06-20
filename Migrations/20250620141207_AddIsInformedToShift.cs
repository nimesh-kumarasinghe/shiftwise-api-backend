using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftWiseAI.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddIsInformedToShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInformed",
                table: "Shifts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInformed",
                table: "Shifts");
        }
    }
}
