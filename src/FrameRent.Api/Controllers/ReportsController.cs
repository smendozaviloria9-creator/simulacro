using FrameRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize(Roles = "Staff")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("top-equipment")]
    public async Task<IActionResult> GetTopEquipment([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var report = await _reportService.GetTopEquipmentReportAsync(limit, cancellationToken);
        return Ok(report);
    }
}
