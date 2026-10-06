using System.ComponentModel.DataAnnotations;

namespace PROG_Part2_RaceDay_API.DTOs;

public class CreateEnrolmentRequest
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}