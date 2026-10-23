using FrameRent.Domain.Entities;
using FrameRent.Domain.Enums;
using FrameRent.Domain.Interfaces;
using FrameRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrameRent.Infrastructure.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly ApplicationDbContext _context;

    public RentalRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Rental?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Rentals
            .Include(r => r.Equipment)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Rental>> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        return await _context.Rentals
            .Include(r => r.Equipment)
            .Where(r => r.ClientId == clientId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Rental>> GetAllAsync(RentalStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Rentals.Include(r => r.Equipment).AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query.OrderByDescending(r => r.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public async Task<int> CountActiveByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        return await _context.Rentals.CountAsync(
            r => r.ClientId == clientId &&
                 (r.Status == RentalStatus.Pending || r.Status == RentalStatus.Approved),
            cancellationToken);
    }

    public async Task<int> GetRentedUnitsInDateRangeAsync(int equipmentId, DateOnly from, DateOnly to, int? excludeRentalId = null, CancellationToken cancellationToken = default)
    {
        // Rentals that overlap with [from, to] and are Approved or Delivered
        // Overlap condition: r.StartDate <= to && r.EndDate >= from
        var query = _context.Rentals.Where(r =>
            r.EquipmentId == equipmentId &&
            (r.Status == RentalStatus.Approved || r.Status == RentalStatus.Delivered) &&
            r.StartDate <= to &&
            r.EndDate >= from);

        if (excludeRentalId.HasValue)
            query = query.Where(r => r.Id != excludeRentalId.Value);

        return await query.SumAsync(r => r.Quantity, cancellationToken);
    }

    public async Task<IEnumerable<KeyValuePair<Equipment, int>>> GetTopRentedEquipmentAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        var top = await _context.Rentals
            .Where(r => r.Status != RentalStatus.Cancelled && r.Status != RentalStatus.Rejected)
            .GroupBy(r => r.Equipment)
            .Select(g => new
            {
                Equipment = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return top.Select(x => new KeyValuePair<Equipment, int>(x.Equipment, x.Count));
    }

    public async Task AddAsync(Rental rental, CancellationToken cancellationToken = default)
    {
        await _context.Rentals.AddAsync(rental, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Rental rental, CancellationToken cancellationToken = default)
    {
        _context.Rentals.Update(rental);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
