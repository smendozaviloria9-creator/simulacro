using System.Security.Claims;
using FrameRent.Application.DTOs.Rental;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Api.Controllers;

[ApiController]
[Route("api/v1/rentals")]
[Authorize]
public class RentalsController : ControllerBase
{
    private readonly IRentalService _rentalService;

    public RentalsController(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> CreateRental([FromBody] RentalRequestDto dto, CancellationToken cancellationToken)
    {
        string clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var rental = await _rentalService.CreateRentalAsync(clientId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = rental.Id }, rental);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyRentals(CancellationToken cancellationToken)
    {
        string clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var rentals = await _rentalService.GetRentalsByClientAsync(clientId, cancellationToken);
        return Ok(rentals);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        bool isStaff = User.IsInRole("Staff");

        var rental = await _rentalService.GetRentalByIdAsync(id, currentUserId, isStaff, cancellationToken);
        if (rental == null)
            return NotFound(new { message = $"Rental with ID {id} not found." });

        return Ok(rental);
    }

    [HttpPatch("{id:int}/cancel")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> CancelRental(int id, CancellationToken cancellationToken)
    {
        string clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        await _rentalService.CancelRentalAsync(id, clientId, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> GetAll([FromQuery] RentalStatus? status, CancellationToken cancellationToken)
    {
        var rentals = await _rentalService.GetAllRentalsAsync(status, cancellationToken);
        return Ok(rentals);
    }

    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        await _rentalService.ApproveRentalAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Reject(int id, CancellationToken cancellationToken)
    {
        await _rentalService.RejectRentalAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/deliver")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Deliver(int id, CancellationToken cancellationToken)
    {
        await _rentalService.DeliverRentalAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/return")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Return(int id, [FromBody] ReturnRentalRequestDto? request, CancellationToken cancellationToken)
    {
        var result = await _rentalService.ReturnRentalAsync(id, request?.ReturnDate, cancellationToken);
        return Ok(result);
    }
}
