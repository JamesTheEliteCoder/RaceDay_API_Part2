using System.ComponentModel.DataAnnotations;

namespace PROG_Part2_RaceDay_API.DTOs;

public class UpdateProfileRequest
{
    [Required, StringLength(30)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string LastName { get; set; } = string.Empty;

    [Required, StringLength(11)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public DateOnly? DateOfBirth { get; set; }
}