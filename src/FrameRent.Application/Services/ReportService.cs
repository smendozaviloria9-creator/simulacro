using FrameRent.Application.DTOs.Equipment;
using FrameRent.Application.DTOs.Report;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Interfaces;

namespace FrameRent.Application.Services;

public class ReportService : IReportService
{
    private readonly IRentalRepository _rentalRepository;

    public ReportService(IRentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
    }

    public async Task<IEnumerable<TopEquipmentReportDto>> GetTopEquipmentReportAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 50);
        var rawData = await _rentalRepository.GetTopRentedEquipmentAsync(limit, cancellationToken);

        return rawData.Select(pair => new TopEquipmentReportDto(
            Equipment: new EquipmentDto(
                pair.Key.Id,
                pair.Key.Name,
                pair.Key.Category,
                pair.Key.Description,
                pair.Key.DailyRate,
                pair.Key.TotalUnits,
                pair.Key.IsActive
            ),
            TotalRentalsCount: pair.Value
        ));
    }
}
