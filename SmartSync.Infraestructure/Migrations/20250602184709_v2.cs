using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSync.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Acoes",
                table: "TipoDispositivos");

            migrationBuilder.AddColumn<bool>(
                name: "Ligado",
                table: "Dispositivos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ligado",
                table: "Dispositivos");

            migrationBuilder.AddColumn<string>(
                name: "Acoes",
                table: "TipoDispositivos",
                type: "TEXT",
                nullable: true);
        }
    }
}
