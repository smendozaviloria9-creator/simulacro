using FrameRent.Domain.Entities;
using FrameRent.Domain.Enums;
using FrameRent.Domain.Interfaces;
using FrameRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrameRent.Infrastructure.Repositories;

public class EquipmentRepository : IEquipmentRepository
{
    private readonly ApplicationDbContext _context;

    public EquipmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Equipments.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Equipment>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Equipments
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Equipments
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasActiveRentalsAsync(int equipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Rentals.AnyAsync(
            r => r.EquipmentId == equipmentId &&
                 (r.Status == RentalStatus.Approved || r.Status == RentalStatus.Delivered),
            cancellationToken);
    }

    public async Task AddAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        await _context.Equipments.AddAsync(equipment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        _context.Equipments.Update(equipment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
