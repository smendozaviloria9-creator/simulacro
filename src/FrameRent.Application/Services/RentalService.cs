using FrameRent.Application.DTOs.Rental;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Entities;
using FrameRent.Domain.Enums;
using FrameRent.Domain.Exceptions;
using FrameRent.Domain.Interfaces;

namespace FrameRent.Application.Services;

public class RentalService : IRentalService
{
    private const int MaxActiveRentalsPerClient = 3;

    private readonly IRentalRepository _rentalRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IDateTimeProvider _clock;

    public RentalService(
        IRentalRepository rentalRepository,
        IEquipmentRepository equipmentRepository,
        IDateTimeProvider clock)
    {
        _rentalRepository = rentalRepository;
        _equipmentRepository = equipmentRepository;
        _clock = clock;
    }

    public async Task<RentalResponseDto> CreateRentalAsync(string clientId, RentalRequestDto dto, CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(dto.EquipmentId, cancellationToken)
            ?? throw new NotFoundException($"Equipment with ID {dto.EquipmentId} not found.");

        if (!equipment.IsActive)
            throw new DomainException("Cannot rent an inactive equipment.");

        // Rule 4: Client cannot have more than 3 pending or approved rentals
        int activeRentalsCount = await _rentalRepository.CountActiveByClientIdAsync(clientId, cancellationToken);
        if (activeRentalsCount >= MaxActiveRentalsPerClient)
            throw new DomainException("Client limit reached: you cannot have more than 3 pending or approved rentals simultaneously.");

        // Rule 2: Check availability in the date range
        int rentedUnits = await _rentalRepository.GetRentedUnitsInDateRangeAsync(dto.EquipmentId, dto.StartDate, dto.EndDate, null, cancellationToken);
        int availableUnits = equipment.TotalUnits - rentedUnits;
        if (dto.Quantity > availableUnits)
            throw new DomainException($"Not enough units available for the selected dates. Available: {Math.Max(0, availableUnits)}, Requested: {dto.Quantity}.");

        var rental = Rental.Create(clientId, equipment, dto.Quantity, dto.StartDate, dto.EndDate, _clock.Today);

        await _rentalRepository.AddAsync(rental, cancellationToken);
        return MapToDto(rental);
    }

    public async Task<IEnumerable<RentalResponseDto>> GetRentalsByClientAsync(string clientId, CancellationToken cancellationToken = default)
    {
        var rentals = await _rentalRepository.GetByClientIdAsync(clientId, cancellationToken);
        return rentals.Select(MapToDto);
    }

    public async Task<RentalResponseDto?> GetRentalByIdAsync(int id, string currentUserId, bool isStaff, CancellationToken cancellationToken = default)
    {
        var rental = await _rentalRepository.GetByIdAsync(id, cancellationToken);
        if (rental == null)
            return null;

        if (!isStaff && rental.ClientId != currentUserId)
            throw new ForbiddenException("You do not have permission to view another client's rental.");

        return MapToDto(rental);
    }

    public async Task<IEnumerable<RentalResponseDto>> GetAllRentalsAsync(RentalStatus? status = null, CancellationToken cancellationToken = default)
    {
        var rentals = await _rentalRepository.GetAllAsync(status, cancellationToken);
        return rentals.Select(MapToDto);
    }

    public async Task CancelRentalAsync(int id, string clientId, CancellationToken cancellationToken = default)
    {
        var rental = await GetRentalOrThrowAsync(id, cancellationToken);

        rental.Cancel(clientId, _clock.Today);

        await _rentalRepository.UpdateAsync(rental, cancellationToken);
    }

    public async Task ApproveRentalAsync(int id, CancellationToken cancellationToken = default)
    {
        var rental = await GetRentalOrThrowAsync(id, cancellationToken);

        // Rule 6: Revalidate availability at approval time
        int rentedUnits = await _rentalRepository.GetRentedUnitsInDateRangeAsync(
            rental.EquipmentId, rental.StartDate, rental.EndDate, rental.Id, cancellationToken);

        var equipment = await _equipmentRepository.GetByIdAsync(rental.EquipmentId, cancellationToken);
        if (equipment == null || !equipment.IsActive)
            throw new DomainException("Equipment is no longer available or has been deactivated.");

        int availableUnits = equipment.TotalUnits - rentedUnits;
        if (rental.Quantity > availableUnits)
            throw new DomainException($"Cannot approve rental: only {Math.Max(0, availableUnits)} unit(s) are available for the requested period.");

        rental.Approve();
        await _rentalRepository.UpdateAsync(rental, cancellationToken);
    }

    public async Task RejectRentalAsync(int id, CancellationToken cancellationToken = default)
    {
        var rental = await GetRentalOrThrowAsync(id, cancellationToken);

        rental.Reject();
        await _rentalRepository.UpdateAsync(rental, cancellationToken);
    }

    public async Task DeliverRentalAsync(int id, CancellationToken cancellationToken = default)
    {
        var rental = await GetRentalOrThrowAsync(id, cancellationToken);

        rental.Deliver();
        await _rentalRepository.UpdateAsync(rental, cancellationToken);
    }

    public async Task<RentalResponseDto> ReturnRentalAsync(int id, DateOnly? returnDate, CancellationToken cancellationToken = default)
    {
        var rental = await GetRentalOrThrowAsync(id, cancellationToken);

        rental.Return(returnDate ?? _clock.Today);

        await _rentalRepository.UpdateAsync(rental, cancellationToken);
        return MapToDto(rental);
    }

    private async Task<Rental> GetRentalOrThrowAsync(int id, CancellationToken cancellationToken)
    {
        return await _rentalRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Rental with ID {id} not found.");
    }

    private static RentalResponseDto MapToDto(Rental r) =>
        new(
            Id: r.Id,
            ClientId: r.ClientId,
            ClientEmail: null,
            EquipmentId: r.EquipmentId,
            EquipmentName: r.Equipment?.Name ?? string.Empty,
            Quantity: r.Quantity,
            StartDate: r.StartDate,
            EndDate: r.EndDate,
            DailyRateAtBooking: r.DailyRateAtBooking,
            TotalCost: r.TotalCost,
            LateFee: r.LateFee,
            Status: r.Status,
            ReturnedDate: r.ReturnedDate,
            CreatedAtUtc: r.CreatedAtUtc
        );
}
