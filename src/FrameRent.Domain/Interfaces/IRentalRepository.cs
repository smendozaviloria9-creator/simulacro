using FrameRent.Domain.Entities;
using FrameRent.Domain.Enums;

namespace FrameRent.Domain.Interfaces;

public interface IRentalRepository
{
    Task<Rental?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Rental>> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Rental>> GetAllAsync(RentalStatus? status = null, CancellationToken cancellationToken = default);
    Task<int> CountActiveByClientIdAsync(string clientId, CancellationToken cancellationToken = default);
    Task<int> GetRentedUnitsInDateRangeAsync(int equipmentId, DateOnly from, DateOnly to, int? excludeRentalId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<KeyValuePair<Equipment, int>>> GetTopRentedEquipmentAsync(int limit = 10, CancellationToken cancellationToken = default);
    Task AddAsync(Rental rental, CancellationToken cancellationToken = default);
    Task UpdateAsync(Rental rental, CancellationToken cancellationToken = default);
}
