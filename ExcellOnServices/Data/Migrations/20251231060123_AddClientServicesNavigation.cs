using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcellOnServices.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientServicesNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId1",
                table: "ClientServices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientServices_ClientId1",
                table: "ClientServices",
                column: "ClientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientServices_Clients_ClientId1",
                table: "ClientServices",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientServices_Clients_ClientId1",
                table: "ClientServices");

            migrationBuilder.DropIndex(
                name: "IX_ClientServices_ClientId1",
                table: "ClientServices");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "ClientServices");
        }
    }
}
