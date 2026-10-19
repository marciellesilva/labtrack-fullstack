using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LabTrack.Application.Dtos;
using LabTrack.Domain.Entities;
using LabTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LabTrack.IntegrationTests;

/// <summary>
/// Sobe a API em memória trocando o SQL Server por SQLite em memória.
/// </summary>
public sealed class LabTrackApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Database:MigrateOnStartup", "false");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<LabTrackDbContext>>();
            _connection.Open();
            services.AddDbContext<LabTrackDbContext>(o => o.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<LabTrackDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}

public class SamplesApiTests(LabTrackApiFactory factory) : IClassFixture<LabTrackApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Get_samples_returns_seed_data()
    {
        var samples = await _client.GetFromJsonAsync<List<SampleSummaryDto>>("/api/samples", Json);

        Assert.NotNull(samples);
        Assert.Contains(samples, s => s.Code == "AM-2026-0001");
    }

    [Fact]
    public async Task Search_by_status_returns_only_matching_samples()
    {
        var samples = await _client.GetFromJsonAsync<List<SampleSummaryDto>>("/api/samples?status=Pending", Json);

        Assert.NotNull(samples);
        Assert.NotEmpty(samples);
        Assert.All(samples, s => Assert.Equal(SampleStatus.Pending, s.Status));
    }

    [Fact]
    public async Task Full_flow_create_sample_add_out_of_spec_result_and_get_rejected()
    {
        var create = await _client.PostAsJsonAsync("/api/samples", new
        {
            code = "AM-INT-0001",
            productName = "Glicerina Bidestilada",
            batchNumber = "L-77001",
            collectedAt = "2026-10-10T08:00:00Z"
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var sample = await create.Content.ReadFromJsonAsync<SampleDetailDto>(Json);
        Assert.NotNull(sample);

        var addResult = await _client.PostAsJsonAsync($"/api/samples/{sample.Id}/results", new
        {
            parameter = "Pureza", unit = "%", value = 98.1, minLimit = 99.5, analyst = "Marcielle"
        });
        Assert.Equal(HttpStatusCode.Created, addResult.StatusCode);

        var detail = await _client.GetFromJsonAsync<SampleDetailDto>($"/api/samples/{sample.Id}", Json);
        Assert.Equal(SampleStatus.Rejected, detail!.Status);
        Assert.False(Assert.Single(detail.Results).IsWithinSpec);
    }

    [Fact]
    public async Task Create_with_invalid_payload_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/samples", new { code = "", productName = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_with_duplicate_code_returns_409()
    {
        var response = await _client.PostAsJsonAsync("/api/samples", new
        {
            code = "AM-2026-0001", productName = "Duplicada", batchNumber = "L-1", collectedAt = "2026-10-10T08:00:00Z"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Get_unknown_sample_returns_404_problem_details()
    {
        var response = await _client.GetAsync("/api/samples/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Invalid_spec_limits_return_422()
    {
        var response = await _client.PostAsJsonAsync("/api/samples/1/results", new
        {
            parameter = "pH", value = 7, minLimit = 8, maxLimit = 6, analyst = "Marcielle"
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_returns_indicators()
    {
        var dashboard = await _client.GetFromJsonAsync<DashboardDto>("/api/dashboard", Json);

        Assert.NotNull(dashboard);
        Assert.True(dashboard.TotalSamples >= 4);
        Assert.NotEmpty(dashboard.RecentOutOfSpec);
    }

    [Fact]
    public async Task Health_check_is_healthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
