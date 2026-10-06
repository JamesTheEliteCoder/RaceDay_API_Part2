using System.ComponentModel.DataAnnotations;

namespace PROG_Part2_RaceDay_API.DTOs;

public class CreateCategoryRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Age|Distance)$",
        ErrorMessage = "Category type must be Age or Distance.")]
    public string CategoryType { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int RouteId { get; set; }

    [Range(0, int.MaxValue)]
    public int? MinimumAge { get; set; }

    [Range(0, int.MaxValue)]
    public int? MaximumAge { get; set; }
}