namespace LabTrack.Domain.Entities;

/// <summary>
/// Resultado de uma análise físico-química (ex.: pH, viscosidade, concentração)
/// com seus limites de especificação.
/// </summary>
public class AnalysisResult
{
    public int Id { get; set; }
    public int SampleId { get; set; }
    public Sample? Sample { get; set; }

    /// <summary>Parâmetro analisado, ex.: "pH", "Viscosidade", "Teor de NaOH".</summary>
    public string Parameter { get; set; } = string.Empty;

    /// <summary>Unidade de medida, ex.: "cP", "%", "g/cm³". Vazio para adimensionais como pH.</summary>
    public string Unit { get; set; } = string.Empty;

    public decimal Value { get; set; }

    /// <summary>Limite inferior de especificação (opcional).</summary>
    public decimal? MinLimit { get; set; }

    /// <summary>Limite superior de especificação (opcional).</summary>
    public decimal? MaxLimit { get; set; }

    public string Analyst { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }

    /// <summary>
    /// Indica se o valor está dentro dos limites (limites ausentes não restringem).
    /// </summary>
    public bool IsWithinSpec =>
        (MinLimit is null || Value >= MinLimit) &&
        (MaxLimit is null || Value <= MaxLimit);
}
