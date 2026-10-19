using LabTrack.Domain.Entities;

namespace LabTrack.UnitTests;

public class SampleDomainTests
{
    [Theory]
    [InlineData(7.0, 6.5, 7.5, true)]   // dentro da faixa
    [InlineData(6.5, 6.5, 7.5, true)]   // limite inferior é inclusivo
    [InlineData(7.5, 6.5, 7.5, true)]   // limite superior é inclusivo
    [InlineData(6.4, 6.5, 7.5, false)]  // abaixo do mínimo
    [InlineData(7.6, 6.5, 7.5, false)]  // acima do máximo
    public void IsWithinSpec_respects_min_and_max(decimal value, decimal min, decimal max, bool expected)
    {
        var result = new AnalysisResult { Value = value, MinLimit = min, MaxLimit = max };

        Assert.Equal(expected, result.IsWithinSpec);
    }

    [Fact]
    public void IsWithinSpec_ignores_missing_limits()
    {
        Assert.True(new AnalysisResult { Value = 1000m }.IsWithinSpec);
        Assert.True(new AnalysisResult { Value = 10m, MaxLimit = 50m }.IsWithinSpec);
        Assert.False(new AnalysisResult { Value = 60m, MaxLimit = 50m }.IsWithinSpec);
    }

    [Fact]
    public void New_sample_is_pending()
    {
        Assert.Equal(SampleStatus.Pending, new Sample().Status);
    }

    [Fact]
    public void Sample_is_approved_when_all_results_are_within_spec()
    {
        var sample = new Sample();
        sample.AddResult(new AnalysisResult { Parameter = "pH", Value = 7m, MinLimit = 6m, MaxLimit = 8m });
        sample.AddResult(new AnalysisResult { Parameter = "Viscosidade", Value = 120m, MaxLimit = 150m });

        Assert.Equal(SampleStatus.Approved, sample.Status);
    }

    [Fact]
    public void Sample_is_rejected_when_any_result_is_out_of_spec()
    {
        var sample = new Sample();
        sample.AddResult(new AnalysisResult { Parameter = "pH", Value = 7m, MinLimit = 6m, MaxLimit = 8m });
        sample.AddResult(new AnalysisResult { Parameter = "Concentração", Value = 90m, MinLimit = 95m });

        Assert.Equal(SampleStatus.Rejected, sample.Status);
    }
}
