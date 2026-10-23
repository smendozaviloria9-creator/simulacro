using FrameRent.Application.DTOs.Auth;
using FrameRent.Application.DTOs.Equipment;
using FrameRent.Application.DTOs.Rental;
using FrameRent.Application.DTOs.Report;
using FrameRent.Domain.Enums;

namespace FrameRent.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}

public interface IEquipmentService
{
    Task<IEnumerable<EquipmentDto>> GetActiveEquipmentsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EquipmentDto>> GetAllEquipmentsAsync(CancellationToken cancellationToken = default);
    Task<EquipmentDto?> GetEquipmentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EquipmentAvailabilityDto> CheckAvailabilityAsync(int equipmentId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<EquipmentDto> CreateEquipmentAsync(EquipmentCreateDto dto, CancellationToken cancellationToken = default);
    Task<EquipmentDto> UpdateEquipmentAsync(int id, EquipmentUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeactivateEquipmentAsync(int id, CancellationToken cancellationToken = default);
}

public interface IRentalService
{
    Task<RentalResponseDto> CreateRentalAsync(string clientId, RentalRequestDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<RentalResponseDto>> GetRentalsByClientAsync(string clientId, CancellationToken cancellationToken = default);
    Task<RentalResponseDto?> GetRentalByIdAsync(int id, string currentUserId, bool isStaff, CancellationToken cancellationToken = default);
    Task<IEnumerable<RentalResponseDto>> GetAllRentalsAsync(RentalStatus? status = null, CancellationToken cancellationToken = default);
    Task CancelRentalAsync(int id, string currentUserId, bool isStaff, CancellationToken cancellationToken = default);
    Task ApproveRentalAsync(int id, CancellationToken cancellationToken = default);
    Task RejectRentalAsync(int id, CancellationToken cancellationToken = default);
    Task DeliverRentalAsync(int id, CancellationToken cancellationToken = default);
    Task<RentalResponseDto> ReturnRentalAsync(int id, DateOnly? returnDate, CancellationToken cancellationToken = default);
}

public interface IReportService
{
    Task<IEnumerable<TopEquipmentReportDto>> GetTopEquipmentReportAsync(int limit = 10, CancellationToken cancellationToken = default);
}
