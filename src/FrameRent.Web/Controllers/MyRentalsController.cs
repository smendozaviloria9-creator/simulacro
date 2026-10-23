using System.Security.Claims;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Web.Controllers;

[Authorize(Roles = "Client")]
public class MyRentalsController : Controller
{
    private readonly IRentalService _rentalService;

    public MyRentalsController(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var rentals = await _rentalService.GetRentalsByClientAsync(clientId);
        return View(rentals);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        string clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        try
        {
            await _rentalService.CancelRentalAsync(id, clientId, isStaff: false);
            TempData["SuccessMessage"] = $"Rental #{id} was cancelled successfully.";
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
