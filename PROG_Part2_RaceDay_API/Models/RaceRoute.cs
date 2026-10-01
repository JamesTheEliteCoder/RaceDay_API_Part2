namespace PROG_Part2_RaceDay_API.Models;

public class RaceRoute
{
    public int RouteId { get; set; }
    public int EventId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string AreasCovered { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public string StartLocation { get; set; } = string.Empty;
    public string FinishLocation { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MapUrl { get; set; }
}