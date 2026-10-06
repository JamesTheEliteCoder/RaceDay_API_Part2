using System.ComponentModel.DataAnnotations;

namespace PROG_Part2_RaceDay_API.DTOs;

public class RecordResultRequest
{
    [Range(1, int.MaxValue)]
    public int FinishingPosition { get; set; }

    public TimeOnly FinishTime { get; set; }
}