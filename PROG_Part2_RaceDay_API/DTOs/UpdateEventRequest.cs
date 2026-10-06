using System.ComponentModel.DataAnnotations;

namespace PROG_Part2_RaceDay_API.DTOs;

public class UpdateEventRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public DateOnly EventDate { get; set; }

    [Required]
    public string Venue { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string Province { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal DistanceKm { get; set; }

    [RegularExpression("^(Run|Walk|Cycle)$",
        ErrorMessage = "Event type must be Run, Walk, or Cycle.")]
    public string EventType { get; set; } = string.Empty;
}