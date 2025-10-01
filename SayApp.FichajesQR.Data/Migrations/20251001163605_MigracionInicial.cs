using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SayApp.FichajesQR.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    GuidAD = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaBaja = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmpleadosQR",
                columns: table => new
                {
                    EmpleadoQRId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    CodigoQR = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaDesactivacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DesactivadoPor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpleadosQR", x => x.EmpleadoQRId);
                    table.ForeignKey(
                        name: "FK_EmpleadosQR_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fichajes",
                columns: table => new
                {
                    FichajeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoQR = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EmpleadoId = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    ErrorDescripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TimestampLectura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimestampProcesado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimestampEnviado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Oficina = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fichajes", x => x.FichajeId);
                    table.ForeignKey(
                        name: "FK_Fichajes_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_EmpleadoId",
                table: "Empleados",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadosQR_EmpleadoId",
                table: "EmpleadosQR",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Fichajes_EmpleadoId",
                table: "Fichajes",
                column: "EmpleadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpleadosQR");

            migrationBuilder.DropTable(
                name: "Fichajes");

            migrationBuilder.DropTable(
                name: "Empleados");
        }
    }
}
