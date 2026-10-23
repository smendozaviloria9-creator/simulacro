namespace FrameRent.Application.DTOs.Equipment;

public record EquipmentDto(
    int Id,
    string Name,
    string Category,
    string Description,
    decimal DailyRate,
    int TotalUnits,
    bool IsActive
);

public record EquipmentCreateDto(
    string Name,
    string Category,
    string Description,
    decimal DailyRate,
    int TotalUnits
);

public record EquipmentUpdateDto(
    string Name,
    string Category,
    string Description,
    decimal DailyRate,
    int TotalUnits
);

public record EquipmentAvailabilityDto(
    int EquipmentId,
    string EquipmentName,
    DateOnly From,
    DateOnly To,
    int TotalUnits,
    int RentedUnits,
    int AvailableUnits
);
