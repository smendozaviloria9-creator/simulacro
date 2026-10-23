using FrameRent.Application.DTOs.Equipment;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Entities;
using FrameRent.Domain.Exceptions;
using FrameRent.Domain.Interfaces;

namespace FrameRent.Application.Services;

public class EquipmentService : IEquipmentService
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IRentalRepository _rentalRepository;

    public EquipmentService(IEquipmentRepository equipmentRepository, IRentalRepository rentalRepository)
    {
        _equipmentRepository = equipmentRepository;
        _rentalRepository = rentalRepository;
    }

    public async Task<IEnumerable<EquipmentDto>> GetActiveEquipmentsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _equipmentRepository.GetAllActiveAsync(cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<IEnumerable<EquipmentDto>> GetAllEquipmentsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _equipmentRepository.GetAllAsync(cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<EquipmentDto?> GetEquipmentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id, cancellationToken);
        return equipment == null ? null : MapToDto(equipment);
    }

    public async Task<EquipmentAvailabilityDto> CheckAvailabilityAsync(int equipmentId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment == null)
            throw new NotFoundException($"Equipment with ID {equipmentId} not found.");

        if (to <= from)
            throw new DomainException("End date ('to') must be after start date ('from').");

        int rentedUnits = await _rentalRepository.GetRentedUnitsInDateRangeAsync(equipmentId, from, to, null, cancellationToken);
        int availableUnits = Math.Max(0, equipment.TotalUnits - rentedUnits);

        return new EquipmentAvailabilityDto(
            EquipmentId: equipment.Id,
            EquipmentName: equipment.Name,
            From: from,
            To: to,
            TotalUnits: equipment.TotalUnits,
            RentedUnits: rentedUnits,
            AvailableUnits: availableUnits
        );
    }

    public async Task<EquipmentDto> CreateEquipmentAsync(EquipmentCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new DomainException("Equipment name is required.");

        if (dto.DailyRate <= 0)
            throw new DomainException("Daily rate must be greater than zero.");

        if (dto.TotalUnits <= 0)
            throw new DomainException("Total units must be greater than zero.");

        var equipment = new Equipment
        {
            Name = dto.Name.Trim(),
            Category = dto.Category.Trim(),
            Description = dto.Description.Trim(),
            DailyRate = dto.DailyRate,
            TotalUnits = dto.TotalUnits,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _equipmentRepository.AddAsync(equipment, cancellationToken);
        return MapToDto(equipment);
    }

    public async Task<EquipmentDto> UpdateEquipmentAsync(int id, EquipmentUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id, cancellationToken);
        if (equipment == null)
            throw new NotFoundException($"Equipment with ID {id} not found.");

        equipment.Update(dto.Name, dto.Category, dto.Description, dto.DailyRate, dto.TotalUnits);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        return MapToDto(equipment);
    }

    public async Task DeactivateEquipmentAsync(int id, CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id, cancellationToken);
        if (equipment == null)
            throw new NotFoundException($"Equipment with ID {id} not found.");

        bool hasActiveRentals = await _equipmentRepository.HasActiveRentalsAsync(id, cancellationToken);
        equipment.Deactivate(hasActiveRentals);

        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);
    }

    private static EquipmentDto MapToDto(Equipment e) =>
        new(e.Id, e.Name, e.Category, e.Description, e.DailyRate, e.TotalUnits, e.IsActive);
}
