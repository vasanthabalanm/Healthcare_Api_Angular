using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthCare_Api_C_.Migrations
{
    /// <inheritdoc />
    public partial class CardioCares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AdminId",
                table: "Appointments",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Admins_AdminId",
                table: "Appointments",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Admins_AdminId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_AdminId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Appointments");
        }
    }
}
