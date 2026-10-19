using LabTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabTrack.Infrastructure.Persistence;

/// <summary>
/// Dados iniciais de exemplo (via migrations) para a aplicação já abrir com conteúdo.
/// </summary>
internal static class SeedData
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        // Status é derivado dos resultados; aqui é informado explicitamente
        // porque o seed não passa pela regra de domínio.
        modelBuilder.Entity<Sample>().HasData(
            new { Id = 1, Code = "AM-2026-0001", ProductName = "Soda Cáustica 50%", BatchNumber = "L-24051",
                  CollectedAt = new DateTime(2026, 9, 1, 8, 30, 0, DateTimeKind.Utc), Notes = (string?)"Tanque T-101",
                  Status = SampleStatus.Approved },
            new { Id = 2, Code = "AM-2026-0002", ProductName = "Ácido Sulfúrico 98%", BatchNumber = "L-24052",
                  CollectedAt = new DateTime(2026, 9, 2, 9, 15, 0, DateTimeKind.Utc), Notes = (string?)null,
                  Status = SampleStatus.Rejected },
            new { Id = 3, Code = "AM-2026-0003", ProductName = "Resina PVC", BatchNumber = "L-24060",
                  CollectedAt = new DateTime(2026, 9, 3, 14, 0, 0, DateTimeKind.Utc), Notes = (string?)"Linha de extrusão 2",
                  Status = SampleStatus.Approved },
            new { Id = 4, Code = "AM-2026-0004", ProductName = "Hipoclorito de Sódio 12%", BatchNumber = "L-24061",
                  CollectedAt = new DateTime(2026, 9, 4, 10, 45, 0, DateTimeKind.Utc), Notes = (string?)null,
                  Status = SampleStatus.Pending });

        modelBuilder.Entity<AnalysisResult>().HasData(
            Result(1, 1, "Teor de NaOH", "%", 49.8m, 49.0m, 51.0m, "2026-09-01T11:00:00Z"),
            Result(2, 1, "Densidade", "g/cm³", 1.525m, 1.510m, 1.540m, "2026-09-01T11:20:00Z"),
            Result(3, 2, "Concentração", "%", 96.2m, 97.5m, 99.0m, "2026-09-02T13:00:00Z"),
            Result(4, 2, "Ferro", "ppm", 35m, null, 50m, "2026-09-02T13:30:00Z"),
            Result(5, 3, "Valor K", "", 65.4m, 64.0m, 67.0m, "2026-09-03T16:10:00Z"),
            Result(6, 3, "Umidade", "%", 0.12m, null, 0.30m, "2026-09-03T16:40:00Z"));
    }

    private static AnalysisResult Result(int id, int sampleId, string parameter, string unit,
        decimal value, decimal? min, decimal? max, string analyzedAt) => new()
    {
        Id = id,
        SampleId = sampleId,
        Parameter = parameter,
        Unit = unit,
        Value = value,
        MinLimit = min,
        MaxLimit = max,
        Analyst = "Marcielle Santos",
        AnalyzedAt = DateTime.Parse(analyzedAt, null, System.Globalization.DateTimeStyles.AdjustToUniversal)
    };
}
