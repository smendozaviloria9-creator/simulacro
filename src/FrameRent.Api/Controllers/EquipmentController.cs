using FrameRent.Application.DTOs.Equipment;
using FrameRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Api.Controllers;

[ApiController]
[Route("api/v1/equipment")]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;

    public EquipmentController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllActive(CancellationToken cancellationToken)
    {
        var items = await _equipmentService.GetActiveEquipmentsAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _equipmentService.GetEquipmentByIdAsync(id, cancellationToken);
        if (item == null)
            return NotFound(new { message = $"Equipment with ID {id} not found." });

        return Ok(item);
    }

    [HttpGet("{id:int}/availability")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckAvailability(
        int id,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken)
    {
        var availability = await _equipmentService.CheckAvailabilityAsync(id, from, to, cancellationToken);
        return Ok(availability);
    }

    [HttpPost]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Create([FromBody] EquipmentCreateDto dto, CancellationToken cancellationToken)
    {
        var created = await _equipmentService.CreateEquipmentAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] EquipmentUpdateDto dto, CancellationToken cancellationToken)
    {
        var updated = await _equipmentService.UpdateEquipmentAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        await _equipmentService.DeactivateEquipmentAsync(id, cancellationToken);
        return NoContent();
    }
}
