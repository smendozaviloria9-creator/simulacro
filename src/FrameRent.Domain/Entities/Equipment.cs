using FrameRent.Domain.Exceptions;

namespace FrameRent.Domain.Entities;

public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DailyRate { get; set; }
    public int TotalUnits { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();

    public void Update(string name, string category, string description, decimal dailyRate, int totalUnits)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Equipment name is required.");

        if (dailyRate <= 0)
            throw new DomainException("Daily rate must be greater than zero.");

        if (totalUnits <= 0)
            throw new DomainException("Total units must be greater than zero.");

        Name = name.Trim();
        Category = category.Trim();
        Description = description.Trim();
        DailyRate = dailyRate;
        TotalUnits = totalUnits;
    }

    public void Deactivate(bool hasActiveRentals)
    {
        if (hasActiveRentals)
            throw new DomainException("Equipment with approved or delivered rentals cannot be deactivated.");

        IsActive = false;
    }
}
