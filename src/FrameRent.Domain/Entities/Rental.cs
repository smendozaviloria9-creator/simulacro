using FrameRent.Domain.Enums;
using FrameRent.Domain.Exceptions;

namespace FrameRent.Domain.Entities;

public class Rental
{
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;

    public int Quantity { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal DailyRateAtBooking { get; set; }
    public decimal TotalCost { get; set; }
    public decimal LateFee { get; set; } = 0;
    public RentalStatus Status { get; set; } = RentalStatus.Pending;

    public DateOnly? ReturnedDate { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    protected Rental() { }

    public static Rental Create(
        string clientId,
        Equipment equipment,
        int quantity,
        DateOnly startDate,
        DateOnly endDate,
        DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new DomainException("Client identifier is required.");

        if (equipment == null)
            throw new DomainException("Equipment not found.");

        if (!equipment.IsActive)
            throw new DomainException("Inactive equipment cannot be rented.");

        if (startDate < today)
            throw new DomainException("Start date cannot be in the past.");

        if (endDate <= startDate)
            throw new DomainException("End date must be after start date.");

        int rentalDays = endDate.DayNumber - startDate.DayNumber;
        if (rentalDays > 15)
            throw new DomainException("Rental duration cannot exceed 15 days.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        decimal totalCost = rentalDays * equipment.DailyRate * quantity;

        return new Rental
        {
            ClientId = clientId,
            EquipmentId = equipment.Id,
            Equipment = equipment,
            Quantity = quantity,
            StartDate = startDate,
            EndDate = endDate,
            DailyRateAtBooking = equipment.DailyRate,
            TotalCost = totalCost,
            LateFee = 0,
            Status = RentalStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Approve()
    {
        if (Status != RentalStatus.Pending)
            throw new DomainException($"Cannot approve rental from status '{Status}'. It must be Pending.");

        Status = RentalStatus.Approved;
    }

    public void Reject()
    {
        if (Status != RentalStatus.Pending)
            throw new DomainException($"Cannot reject rental from status '{Status}'. It must be Pending.");

        Status = RentalStatus.Rejected;
    }

    public void Deliver()
    {
        if (Status != RentalStatus.Approved)
            throw new DomainException($"Cannot deliver rental from status '{Status}'. It must be Approved first.");

        Status = RentalStatus.Delivered;
    }

    public void Return(DateOnly returnDate)
    {
        if (Status != RentalStatus.Delivered)
            throw new DomainException($"Cannot return rental from status '{Status}'. It must be in Delivered status.");

        ReturnedDate = returnDate;
        Status = RentalStatus.Returned;

        if (returnDate > EndDate)
        {
            int overdueDays = returnDate.DayNumber - EndDate.DayNumber;
            // Late fee formula: days overdue * daily rate * quantity * 1.5
            LateFee = overdueDays * DailyRateAtBooking * Quantity * 1.5m;
        }
    }

    public void Cancel(string requestingUserId, bool isStaff, DateOnly today)
    {
        if (!isStaff && requestingUserId != ClientId)
            throw new DomainException("You can only cancel your own rentals.");

        if (Status != RentalStatus.Pending && Status != RentalStatus.Approved)
            throw new DomainException($"Cannot cancel rental with status '{Status}'. Only Pending or Approved rentals can be cancelled.");

        if (today >= StartDate)
            throw new DomainException("Rentals can only be cancelled before the start date.");

        Status = RentalStatus.Cancelled;
    }
}
