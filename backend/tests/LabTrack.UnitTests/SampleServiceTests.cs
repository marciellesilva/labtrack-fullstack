using LabTrack.Application.Abstractions;
using LabTrack.Application.Dtos;
using LabTrack.Application.Exceptions;
using LabTrack.Application.Services;
using LabTrack.Domain.Entities;
using LabTrack.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LabTrack.UnitTests;

/// <summary>
/// Testa o serviço com o repositório real sobre SQLite em memória
/// e um publicador de eventos falso.
/// </summary>
public sealed class SampleServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LabTrackDbContext _db;
    private readonly FakeEventPublisher _publisher = new();
    private readonly SampleService _service;

    public SampleServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _db = new LabTrackDbContext(new DbContextOptionsBuilder<LabTrackDbContext>()
            .UseSqlite(_connection).Options);
        _db.Database.EnsureCreated(); // inclui os dados de seed (4 amostras)

        _service = new SampleService(new SampleRepository(_db), _publisher, NullLogger<SampleService>.Instance);
    }

    private static SampleRequest NewSample(string code = "AM-TEST-0001") => new()
    {
        Code = code,
        ProductName = "Etanol Hidratado",
        BatchNumber = "L-99001",
        CollectedAt = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc)
    };

    [Fact]
    public async Task CreateAsync_normalizes_code_and_starts_pending()
    {
        var created = await _service.CreateAsync(NewSample("am-test-0001"));

        Assert.Equal("AM-TEST-0001", created.Code);
        Assert.Equal(SampleStatus.Pending, created.Status);
        Assert.Empty(created.Results);
    }

    [Fact]
    public async Task CreateAsync_rejects_duplicate_code()
    {
        await _service.CreateAsync(NewSample());

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(NewSample()));
    }

    [Fact]
    public async Task AddResultAsync_out_of_spec_rejects_sample_and_publishes_event()
    {
        var sample = await _service.CreateAsync(NewSample());

        var result = await _service.AddResultAsync(sample.Id, new AddResultRequest
        {
            Parameter = "Teor alcoólico", Unit = "%", Value = 90.1m,
            MinLimit = 92.5m, MaxLimit = 94.6m, Analyst = "Marcielle"
        });

        Assert.False(result.IsWithinSpec);
        Assert.Equal(SampleStatus.Rejected, (await _service.GetByIdAsync(sample.Id)).Status);
        var evt = Assert.Single(_publisher.Events);
        Assert.Equal("AM-TEST-0001", evt.SampleCode);
        Assert.Equal("Teor alcoólico", evt.Parameter);
    }

    [Fact]
    public async Task AddResultAsync_within_spec_approves_sample_without_event()
    {
        var sample = await _service.CreateAsync(NewSample());

        await _service.AddResultAsync(sample.Id, new AddResultRequest
        {
            Parameter = "pH", Value = 7.1m, MinLimit = 6m, MaxLimit = 8m, Analyst = "Marcielle"
        });

        Assert.Equal(SampleStatus.Approved, (await _service.GetByIdAsync(sample.Id)).Status);
        Assert.Empty(_publisher.Events);
    }

    [Fact]
    public async Task AddResultAsync_rejects_min_greater_than_max()
    {
        var sample = await _service.CreateAsync(NewSample());

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.AddResultAsync(sample.Id, new AddResultRequest
        {
            Parameter = "pH", Value = 7m, MinLimit = 8m, MaxLimit = 6m, Analyst = "Marcielle"
        }));
    }

    [Fact]
    public async Task GetByIdAsync_throws_when_not_found()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(9999));
    }

    [Fact]
    public async Task SearchAsync_filters_by_term_and_status()
    {
        var byProduct = await _service.SearchAsync("soda", null);
        var rejected = await _service.SearchAsync(null, SampleStatus.Rejected);

        Assert.Equal("AM-2026-0001", Assert.Single(byProduct).Code);
        Assert.All(rejected, s => Assert.Equal(SampleStatus.Rejected, s.Status));
        Assert.NotEmpty(rejected);
    }

    [Fact]
    public async Task GetDashboardAsync_summarizes_seed_data()
    {
        var dashboard = await _service.GetDashboardAsync();

        Assert.Equal(4, dashboard.TotalSamples);
        Assert.Equal(2, dashboard.ApprovedSamples);
        Assert.Equal(1, dashboard.RejectedSamples);
        Assert.Equal(1, dashboard.PendingSamples);
        Assert.Equal(6, dashboard.TotalResults);
        Assert.Equal(1, dashboard.OutOfSpecResults);
        Assert.Equal(66.7m, dashboard.ApprovalRate);
        Assert.Equal("Concentração", Assert.Single(dashboard.RecentOutOfSpec).Parameter);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private sealed class FakeEventPublisher : IEventPublisher
    {
        public List<OutOfSpecDetectedEvent> Events { get; } = new();

        public Task PublishAsync(OutOfSpecDetectedEvent @event, CancellationToken ct = default)
        {
            Events.Add(@event);
            return Task.CompletedTask;
        }
    }
}
