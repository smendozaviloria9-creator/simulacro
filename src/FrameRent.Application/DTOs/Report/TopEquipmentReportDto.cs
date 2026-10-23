using FrameRent.Application.DTOs.Equipment;

namespace FrameRent.Application.DTOs.Report;

public record TopEquipmentReportDto(
    EquipmentDto Equipment,
    int TotalRentalsCount
);
