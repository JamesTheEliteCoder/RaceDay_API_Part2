namespace PROG_Part2_RaceDay_API.DTOs;

public class ParticipantResultResponse
{
    public int ResultId { get; set; }

    public int EventId { get; set; }

    public string EventName { get; set; } = string.Empty;

    public DateOnly EventDate { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int FinishingPosition { get; set; }

    public int TotalCategoryResults { get; set; }

    public TimeOnly FinishTime { get; set; }
}