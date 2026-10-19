using LabTrack.Application.Abstractions;
using LabTrack.Application.Dtos;
using LabTrack.Application.Exceptions;
using LabTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LabTrack.Application.Services;

public interface ISampleService
{
    Task<IReadOnlyList<SampleSummaryDto>> SearchAsync(string? search, SampleStatus? status, CancellationToken ct = default);
    Task<SampleDetailDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SampleDetailDto> CreateAsync(SampleRequest request, CancellationToken ct = default);
    Task<SampleDetailDto> UpdateAsync(int id, SampleRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<AnalysisResultDto> AddResultAsync(int sampleId, AddResultRequest request, CancellationToken ct = default);
    Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default);
}

/// <summary>
/// Casos de uso de amostras e resultados de análise.
/// </summary>
public class SampleService(
    ISampleRepository repository,
    IEventPublisher eventPublisher,
    ILogger<SampleService> logger) : ISampleService
{
    private const int RecentOutOfSpecLimit = 5;

    public async Task<IReadOnlyList<SampleSummaryDto>> SearchAsync(
        string? search, SampleStatus? status, CancellationToken ct = default)
    {
        var samples = await repository.SearchAsync(search?.Trim(), status, ct);
        return samples.Select(ToSummary).ToList();
    }

    public async Task<SampleDetailDto> GetByIdAsync(int id, CancellationToken ct = default)
        => ToDetail(await GetSampleOrThrowAsync(id, ct));

    public async Task<SampleDetailDto> CreateAsync(SampleRequest request, CancellationToken ct = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await repository.CodeExistsAsync(code, null, ct))
            throw new ConflictException($"Já existe uma amostra com o código '{code}'.");

        var sample = new Sample
        {
            Code = code,
            ProductName = request.ProductName.Trim(),
            BatchNumber = request.BatchNumber.Trim(),
            CollectedAt = request.CollectedAt!.Value,
            Notes = request.Notes?.Trim()
        };

        await repository.AddAsync(sample, ct);
        await repository.SaveChangesAsync(ct);
        logger.LogInformation("Amostra {Code} criada (id {Id})", sample.Code, sample.Id);
        return ToDetail(sample);
    }

    public async Task<SampleDetailDto> UpdateAsync(int id, SampleRequest request, CancellationToken ct = default)
    {
        var sample = await GetSampleOrThrowAsync(id, ct);
        var code = request.Code.Trim().ToUpperInvariant();
        if (await repository.CodeExistsAsync(code, id, ct))
            throw new ConflictException($"Já existe uma amostra com o código '{code}'.");

        sample.Code = code;
        sample.ProductName = request.ProductName.Trim();
        sample.BatchNumber = request.BatchNumber.Trim();
        sample.CollectedAt = request.CollectedAt!.Value;
        sample.Notes = request.Notes?.Trim();

        await repository.SaveChangesAsync(ct);
        return ToDetail(sample);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var sample = await GetSampleOrThrowAsync(id, ct);
        repository.Remove(sample);
        await repository.SaveChangesAsync(ct);
        logger.LogInformation("Amostra {Code} removida", sample.Code);
    }

    public async Task<AnalysisResultDto> AddResultAsync(
        int sampleId, AddResultRequest request, CancellationToken ct = default)
    {
        if (request.MinLimit is not null && request.MaxLimit is not null && request.MinLimit > request.MaxLimit)
            throw new BusinessRuleException("O limite mínimo não pode ser maior que o limite máximo.");

        var sample = await GetSampleOrThrowAsync(sampleId, ct);
        var result = new AnalysisResult
        {
            Parameter = request.Parameter.Trim(),
            Unit = request.Unit.Trim(),
            Value = request.Value!.Value,
            MinLimit = request.MinLimit,
            MaxLimit = request.MaxLimit,
            Analyst = request.Analyst.Trim(),
            AnalyzedAt = request.AnalyzedAt ?? DateTime.UtcNow
        };

        sample.AddResult(result);
        await repository.SaveChangesAsync(ct);

        if (!result.IsWithinSpec)
        {
            logger.LogWarning(
                "Resultado fora de especificação: amostra {Code}, {Parameter} = {Value}",
                sample.Code, result.Parameter, result.Value);

            await eventPublisher.PublishAsync(new OutOfSpecDetectedEvent(
                sample.Id, sample.Code, sample.ProductName, sample.BatchNumber,
                result.Parameter, result.Value, result.MinLimit, result.MaxLimit, result.AnalyzedAt), ct);
        }

        return ToResultDto(result);
    }

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var byStatus = await repository.CountByStatusAsync(ct);
        var (totalResults, outOfSpec) = await repository.CountResultsAsync(ct);
        var recent = await repository.GetRecentOutOfSpecAsync(RecentOutOfSpecLimit, ct);

        int Count(SampleStatus s) => byStatus.TryGetValue(s, out var n) ? n : 0;
        var approved = Count(SampleStatus.Approved);
        var rejected = Count(SampleStatus.Rejected);
        var pending = Count(SampleStatus.Pending);
        var analyzed = approved + rejected;

        // Taxa de aprovação considera apenas amostras já analisadas.
        var approvalRate = analyzed == 0 ? 0 : Math.Round(approved * 100m / analyzed, 1);

        return new DashboardDto(
            approved + rejected + pending, pending, approved, rejected,
            totalResults, outOfSpec, approvalRate,
            recent.Select(r => new OutOfSpecItemDto(
                r.SampleId, r.Sample!.Code, r.Sample.ProductName, r.Parameter,
                r.Value, r.Unit, r.MinLimit, r.MaxLimit, r.AnalyzedAt)).ToList());
    }

    private async Task<Sample> GetSampleOrThrowAsync(int id, CancellationToken ct)
        => await repository.GetByIdAsync(id, ct)
           ?? throw new NotFoundException($"Amostra {id} não encontrada.");

    // ---- Mapeamentos entidade -> DTO ----

    private static SampleSummaryDto ToSummary(Sample s) => new(
        s.Id, s.Code, s.ProductName, s.BatchNumber, s.CollectedAt, s.Status,
        s.Results.Count, s.Results.Count(r => !r.IsWithinSpec));

    private static SampleDetailDto ToDetail(Sample s) => new(
        s.Id, s.Code, s.ProductName, s.BatchNumber, s.CollectedAt, s.Notes, s.Status,
        s.Results.OrderByDescending(r => r.AnalyzedAt).Select(ToResultDto).ToList());

    private static AnalysisResultDto ToResultDto(AnalysisResult r) => new(
        r.Id, r.Parameter, r.Unit, r.Value, r.MinLimit, r.MaxLimit,
        r.IsWithinSpec, r.Analyst, r.AnalyzedAt);
}
