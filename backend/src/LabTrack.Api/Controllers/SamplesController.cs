using LabTrack.Application.Dtos;
using LabTrack.Application.Services;
using LabTrack.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LabTrack.Api.Controllers;

[ApiController]
[Route("api/samples")]
[Produces("application/json")]
public class SamplesController(ISampleService service) : ControllerBase
{
    /// <summary>Lista amostras, com busca por código, produto ou lote e filtro por status.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SampleSummaryDto>>> Search(
        [FromQuery] string? search, [FromQuery] SampleStatus? status, CancellationToken ct)
        => Ok(await service.SearchAsync(search, status, ct));

    /// <summary>Detalha uma amostra com seus resultados.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SampleDetailDto>> GetById(int id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    /// <summary>Registra uma nova amostra.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SampleDetailDto>> Create(SampleRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza os dados de uma amostra.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SampleDetailDto>> Update(int id, SampleRequest request, CancellationToken ct)
        => Ok(await service.UpdateAsync(id, request, ct));

    /// <summary>Remove uma amostra e seus resultados.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Registra um resultado de análise. Resultados fora de especificação reprovam a amostra.</summary>
    [HttpPost("{id:int}/results")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AnalysisResultDto>> AddResult(int id, AddResultRequest request, CancellationToken ct)
    {
        var result = await service.AddResultAsync(id, request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }
}

[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController(ISampleService service) : ControllerBase
{
    /// <summary>Indicadores de qualidade e últimos resultados fora de especificação.</summary>
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
        => Ok(await service.GetDashboardAsync(ct));
}
