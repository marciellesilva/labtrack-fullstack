using System.ComponentModel.DataAnnotations;
using LabTrack.Domain.Entities;

namespace LabTrack.Application.Dtos;

/// <summary>Resumo da amostra para listagens.</summary>
public record SampleSummaryDto(
    int Id,
    string Code,
    string ProductName,
    string BatchNumber,
    DateTime CollectedAt,
    SampleStatus Status,
    int ResultsCount,
    int OutOfSpecCount);

/// <summary>Amostra completa, com todos os resultados.</summary>
public record SampleDetailDto(
    int Id,
    string Code,
    string ProductName,
    string BatchNumber,
    DateTime CollectedAt,
    string? Notes,
    SampleStatus Status,
    IReadOnlyList<AnalysisResultDto> Results);

public record AnalysisResultDto(
    int Id,
    string Parameter,
    string Unit,
    decimal Value,
    decimal? MinLimit,
    decimal? MaxLimit,
    bool IsWithinSpec,
    string Analyst,
    DateTime AnalyzedAt);

/// <summary>Dados para criar ou atualizar uma amostra.</summary>
public class SampleRequest
{
    [Required(ErrorMessage = "O código é obrigatório.")]
    [StringLength(20, MinimumLength = 3)]
    [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Use apenas letras maiúsculas, números e hífen.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "O produto é obrigatório.")]
    [StringLength(100)]
    public string ProductName { get; set; } = string.Empty;

    [Required(ErrorMessage = "O lote é obrigatório.")]
    [StringLength(30)]
    public string BatchNumber { get; set; } = string.Empty;

    [Required]
    public DateTime? CollectedAt { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>Dados para registrar um resultado de análise.</summary>
public class AddResultRequest
{
    [Required(ErrorMessage = "O parâmetro é obrigatório.")]
    [StringLength(50)]
    public string Parameter { get; set; } = string.Empty;

    [StringLength(20)]
    public string Unit { get; set; } = string.Empty;

    [Required(ErrorMessage = "O valor é obrigatório.")]
    public decimal? Value { get; set; }

    public decimal? MinLimit { get; set; }
    public decimal? MaxLimit { get; set; }

    [Required(ErrorMessage = "O analista é obrigatório.")]
    [StringLength(100)]
    public string Analyst { get; set; } = string.Empty;

    /// <summary>Data da análise; se omitida, usa o momento atual (UTC).</summary>
    public DateTime? AnalyzedAt { get; set; }
}

/// <summary>Indicadores do painel de qualidade.</summary>
public record DashboardDto(
    int TotalSamples,
    int PendingSamples,
    int ApprovedSamples,
    int RejectedSamples,
    int TotalResults,
    int OutOfSpecResults,
    decimal ApprovalRate,
    IReadOnlyList<OutOfSpecItemDto> RecentOutOfSpec);

public record OutOfSpecItemDto(
    int SampleId,
    string SampleCode,
    string ProductName,
    string Parameter,
    decimal Value,
    string Unit,
    decimal? MinLimit,
    decimal? MaxLimit,
    DateTime AnalyzedAt);
