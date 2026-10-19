using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LabTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Samples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CollectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Samples", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SampleId = table.Column<int>(type: "int", nullable: false),
                    Parameter = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MinLimit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxLimit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Analyst = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisResults_Samples_SampleId",
                        column: x => x.SampleId,
                        principalTable: "Samples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Samples",
                columns: new[] { "Id", "BatchNumber", "Code", "CollectedAt", "Notes", "ProductName", "Status" },
                values: new object[,]
                {
                    { 1, "L-24051", "AM-2026-0001", new DateTime(2026, 9, 1, 8, 30, 0, 0, DateTimeKind.Utc), "Tanque T-101", "Soda Cáustica 50%", "Approved" },
                    { 2, "L-24052", "AM-2026-0002", new DateTime(2026, 9, 2, 9, 15, 0, 0, DateTimeKind.Utc), null, "Ácido Sulfúrico 98%", "Rejected" },
                    { 3, "L-24060", "AM-2026-0003", new DateTime(2026, 9, 3, 14, 0, 0, 0, DateTimeKind.Utc), "Linha de extrusão 2", "Resina PVC", "Approved" },
                    { 4, "L-24061", "AM-2026-0004", new DateTime(2026, 9, 4, 10, 45, 0, 0, DateTimeKind.Utc), null, "Hipoclorito de Sódio 12%", "Pending" }
                });

            migrationBuilder.InsertData(
                table: "AnalysisResults",
                columns: new[] { "Id", "Analyst", "AnalyzedAt", "MaxLimit", "MinLimit", "Parameter", "SampleId", "Unit", "Value" },
                values: new object[,]
                {
                    { 1, "Marcielle Santos", new DateTime(2026, 9, 1, 11, 0, 0, 0, DateTimeKind.Utc), 51.0m, 49.0m, "Teor de NaOH", 1, "%", 49.8m },
                    { 2, "Marcielle Santos", new DateTime(2026, 9, 1, 11, 20, 0, 0, DateTimeKind.Utc), 1.540m, 1.510m, "Densidade", 1, "g/cm³", 1.525m },
                    { 3, "Marcielle Santos", new DateTime(2026, 9, 2, 13, 0, 0, 0, DateTimeKind.Utc), 99.0m, 97.5m, "Concentração", 2, "%", 96.2m },
                    { 4, "Marcielle Santos", new DateTime(2026, 9, 2, 13, 30, 0, 0, DateTimeKind.Utc), 50m, null, "Ferro", 2, "ppm", 35m },
                    { 5, "Marcielle Santos", new DateTime(2026, 9, 3, 16, 10, 0, 0, DateTimeKind.Utc), 67.0m, 64.0m, "Valor K", 3, "", 65.4m },
                    { 6, "Marcielle Santos", new DateTime(2026, 9, 3, 16, 40, 0, 0, DateTimeKind.Utc), 0.30m, null, "Umidade", 3, "%", 0.12m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisResults_AnalyzedAt",
                table: "AnalysisResults",
                column: "AnalyzedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisResults_SampleId",
                table: "AnalysisResults",
                column: "SampleId");

            migrationBuilder.CreateIndex(
                name: "IX_Samples_BatchNumber",
                table: "Samples",
                column: "BatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Samples_Code",
                table: "Samples",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisResults");

            migrationBuilder.DropTable(
                name: "Samples");
        }
    }
}
