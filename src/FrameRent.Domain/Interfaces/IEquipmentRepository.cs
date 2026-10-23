using FrameRent.Domain.Entities;
using FrameRent.Domain.Enums;

namespace FrameRent.Domain.Interfaces;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Equipment>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Equipment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> HasActiveRentalsAsync(int equipmentId, CancellationToken cancellationToken = default);
    Task AddAsync(Equipment equipment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default);
}
