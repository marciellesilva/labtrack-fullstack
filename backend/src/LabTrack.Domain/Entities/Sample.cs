namespace LabTrack.Domain.Entities;

/// <summary>
/// Amostra coletada de um lote de produção para controle de qualidade.
/// </summary>
public class Sample
{
    public int Id { get; set; }

    /// <summary>Código único da amostra, ex.: "AM-2026-0001".</summary>
    public string Code { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime CollectedAt { get; set; }
    public string? Notes { get; set; }
    public SampleStatus Status { get; private set; } = SampleStatus.Pending;

    public List<AnalysisResult> Results { get; set; } = new();

    /// <summary>
    /// Adiciona um resultado e recalcula o status da amostra.
    /// </summary>
    public void AddResult(AnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        Results.Add(result);
        RecalculateStatus();
    }

    /// <summary>
    /// Regra de negócio: sem resultados = Pendente; algum fora de especificação = Reprovada;
    /// caso contrário = Aprovada.
    /// </summary>
    public void RecalculateStatus()
    {
        if (Results.Count == 0)
            Status = SampleStatus.Pending;
        else if (Results.Any(r => !r.IsWithinSpec))
            Status = SampleStatus.Rejected;
        else
            Status = SampleStatus.Approved;
    }
}
