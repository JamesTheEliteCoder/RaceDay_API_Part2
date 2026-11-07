namespace PROG_Part2_RaceDay_API.Models;

public class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    // Stores the URL of the user's profile picture in Azure Blob Storage.
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedOn { get; set; }
}