using LabTrack.Domain.Entities;

namespace LabTrack.Application.Abstractions;

/// <summary>
/// Acesso a dados das amostras. Implementado na camada de Infraestrutura com EF Core.
/// </summary>
public interface ISampleRepository
{
    Task<IReadOnlyList<Sample>> SearchAsync(string? search, SampleStatus? status, CancellationToken ct = default);
    Task<Sample?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? exceptId = null, CancellationToken ct = default);
    Task AddAsync(Sample sample, CancellationToken ct = default);
    void Remove(Sample sample);
    Task<IReadOnlyDictionary<SampleStatus, int>> CountByStatusAsync(CancellationToken ct = default);
    Task<(int Total, int OutOfSpec)> CountResultsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AnalysisResult>> GetRecentOutOfSpecAsync(int take, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

/// <summary>
/// Evento publicado quando um resultado fora de especificação é registrado.
/// </summary>
public record OutOfSpecDetectedEvent(
    int SampleId,
    string SampleCode,
    string ProductName,
    string BatchNumber,
    string Parameter,
    decimal Value,
    decimal? MinLimit,
    decimal? MaxLimit,
    DateTime AnalyzedAt);

/// <summary>
/// Publicador de eventos de integração (ex.: fila AWS SQS).
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync(OutOfSpecDetectedEvent @event, CancellationToken ct = default);
}
