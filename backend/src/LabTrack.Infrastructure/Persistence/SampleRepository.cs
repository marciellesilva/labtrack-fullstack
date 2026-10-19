using LabTrack.Application.Abstractions;
using LabTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabTrack.Infrastructure.Persistence;

public class SampleRepository(LabTrackDbContext db) : ISampleRepository
{
    public async Task<IReadOnlyList<Sample>> SearchAsync(
        string? search, SampleStatus? status, CancellationToken ct = default)
    {
        var query = db.Samples.Include(s => s.Results).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(s =>
                s.Code.ToLower().Contains(term) ||
                s.ProductName.ToLower().Contains(term) ||
                s.BatchNumber.ToLower().Contains(term));
        }

        if (status is not null)
            query = query.Where(s => s.Status == status);

        return await query.OrderByDescending(s => s.CollectedAt).ToListAsync(ct);
    }

    public Task<Sample?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.Samples.Include(s => s.Results).FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<bool> CodeExistsAsync(string code, int? exceptId = null, CancellationToken ct = default)
        => db.Samples.AnyAsync(s => s.Code == code && (exceptId == null || s.Id != exceptId), ct);

    public async Task AddAsync(Sample sample, CancellationToken ct = default)
        => await db.Samples.AddAsync(sample, ct);

    public void Remove(Sample sample) => db.Samples.Remove(sample);

    public async Task<IReadOnlyDictionary<SampleStatus, int>> CountByStatusAsync(CancellationToken ct = default)
        => await db.Samples
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, ct);

    public async Task<(int Total, int OutOfSpec)> CountResultsAsync(CancellationToken ct = default)
    {
        var total = await db.AnalysisResults.CountAsync(ct);
        var outOfSpec = await OutOfSpec(db.AnalysisResults).CountAsync(ct);
        return (total, outOfSpec);
    }

    public async Task<IReadOnlyList<AnalysisResult>> GetRecentOutOfSpecAsync(int take, CancellationToken ct = default)
        => await OutOfSpec(db.AnalysisResults.Include(r => r.Sample).AsNoTracking())
            .OrderByDescending(r => r.AnalyzedAt)
            .Take(take)
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);

    /// <summary>
    /// Mesma regra de <see cref="AnalysisResult.IsWithinSpec"/>, escrita de forma traduzível para SQL.
    /// </summary>
    private static IQueryable<AnalysisResult> OutOfSpec(IQueryable<AnalysisResult> query)
        => query.Where(r =>
            (r.MinLimit != null && r.Value < r.MinLimit) ||
            (r.MaxLimit != null && r.Value > r.MaxLimit));
}
