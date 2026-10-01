namespace PROG_Part2_RaceDay_API.Models;

public class Result
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int FinishingPosition { get; set; }
    public TimeOnly FinishTime { get; set; }
}