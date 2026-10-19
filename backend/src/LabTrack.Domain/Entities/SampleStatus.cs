namespace LabTrack.Domain.Entities;

/// <summary>
/// Situação de uma amostra, derivada automaticamente dos resultados de análise.
/// </summary>
public enum SampleStatus
{
    /// <summary>Amostra registrada, ainda sem resultados.</summary>
    Pending = 0,

    /// <summary>Todos os resultados estão dentro da especificação.</summary>
    Approved = 1,

    /// <summary>Ao menos um resultado está fora da especificação.</summary>
    Rejected = 2
}
