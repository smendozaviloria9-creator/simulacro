using System.ComponentModel.DataAnnotations;

namespace FrameRent.Web.Models;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters and include an uppercase letter, a lowercase letter and a digit.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class RentEquipmentViewModel
{
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public decimal DailyRate { get; set; }
    public int TotalUnits { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; } = 1;

    [Required]
    [Display(Name = "Start Date")]
    public DateOnly StartDate { get; set; }

    [Required]
    [Display(Name = "End Date")]
    public DateOnly EndDate { get; set; }
}
