using System.ComponentModel.DataAnnotations;

namespace PROG_Part2_RaceDay_API.DTOs;

public class RaceRouteRequest
{
    [Required]
    [StringLength(100)]
    public string RouteName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string AreasCovered { get; set; } = string.Empty;

    [Range(0.01, 9999.99)]
    public decimal DistanceKm { get; set; }

    [Required]
    [StringLength(150)]
    public string StartLocation { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string FinishLocation { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? MapUrl { get; set; }
}