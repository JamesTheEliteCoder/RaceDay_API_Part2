namespace PROG_Part2_RaceDay_API.Models;

public class Category
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public int RouteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
}