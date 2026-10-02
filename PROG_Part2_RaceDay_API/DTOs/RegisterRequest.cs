using System.ComponentModel.DataAnnotations;

namespace PROG_Part2_RaceDay_API.DTOs;

public class RegisterRequest
{
    [Required, StringLength(30)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Organiser|Participant)$",
        ErrorMessage = "Role must be Organiser or Participant.")]
    public string Role { get; set; } = string.Empty;

    [Required, StringLength(11)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public DateOnly? DateOfBirth { get; set; }
}