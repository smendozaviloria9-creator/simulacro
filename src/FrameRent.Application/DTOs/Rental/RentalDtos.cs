using FrameRent.Domain.Enums;

namespace FrameRent.Application.DTOs.Rental;

public record RentalRequestDto(
    int EquipmentId,
    int Quantity,
    DateOnly StartDate,
    DateOnly EndDate
);

public record RentalResponseDto(
    int Id,
    string ClientId,
    string? ClientEmail,
    int EquipmentId,
    string EquipmentName,
    int Quantity,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal DailyRateAtBooking,
    decimal TotalCost,
    decimal LateFee,
    RentalStatus Status,
    DateOnly? ReturnedDate,
    DateTime CreatedAtUtc
);

public record ReturnRentalRequestDto(
    DateOnly? ReturnDate
);
