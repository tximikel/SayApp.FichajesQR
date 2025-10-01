using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SayApp.FichajesQR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditoriaYConcurrencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: "Fichajes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Fichajes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "Fichajes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "Fichajes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Fichajes",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "EmpleadosQR",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "EmpleadosQR",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EmpleadosQR",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: "Empleados",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DNI",
                table: "Empleados",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Empleados",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "Empleados",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "Empleados",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Empleados",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreadoPor",
                table: "Fichajes");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Fichajes");

            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "Fichajes");

            migrationBuilder.DropColumn(
                name: "ModificadoPor",
                table: "Fichajes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Fichajes");

            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "EmpleadosQR");

            migrationBuilder.DropColumn(
                name: "ModificadoPor",
                table: "EmpleadosQR");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EmpleadosQR");

            migrationBuilder.DropColumn(
                name: "CreadoPor",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "DNI",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "ModificadoPor",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Empleados");
        }
    }
}
