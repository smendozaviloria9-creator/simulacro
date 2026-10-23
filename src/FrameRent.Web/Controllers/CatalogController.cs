using System.Security.Claims;
using FrameRent.Application.DTOs.Rental;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Exceptions;
using FrameRent.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Web.Controllers;

public class CatalogController : Controller
{
    private readonly IEquipmentService _equipmentService;
    private readonly IRentalService _rentalService;

    public CatalogController(IEquipmentService equipmentService, IRentalService rentalService)
    {
        _equipmentService = equipmentService;
        _rentalService = rentalService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var equipments = await _equipmentService.GetActiveEquipmentsAsync();
        return View(equipments);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, DateOnly? from, DateOnly? to)
    {
        var equipment = await _equipmentService.GetEquipmentByIdAsync(id);
        if (equipment == null || !equipment.IsActive)
            return NotFound();

        ViewBag.From = from;
        ViewBag.To = to;

        if (from.HasValue && to.HasValue)
        {
            try
            {
                var availability = await _equipmentService.CheckAvailabilityAsync(id, from.Value, to.Value);
                ViewBag.Availability = availability;
            }
            catch (DomainException ex)
            {
                ViewBag.AvailabilityError = ex.Message;
            }
        }

        return View(equipment);
    }

    [HttpGet]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Rent(int id)
    {
        var equipment = await _equipmentService.GetEquipmentByIdAsync(id);
        if (equipment == null || !equipment.IsActive)
            return NotFound();

        var model = new RentEquipmentViewModel
        {
            EquipmentId = equipment.Id,
            EquipmentName = equipment.Name,
            DailyRate = equipment.DailyRate,
            TotalUnits = equipment.TotalUnits,
            Quantity = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rent(RentEquipmentViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        string clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        try
        {
            var request = new RentalRequestDto(
                EquipmentId: model.EquipmentId,
                Quantity: model.Quantity,
                StartDate: model.StartDate,
                EndDate: model.EndDate
            );

            await _rentalService.CreateRentalAsync(clientId, request);
            TempData["SuccessMessage"] = "Rental request successfully submitted! Awaiting staff approval.";
            return RedirectToAction("Index", "MyRentals");
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}
