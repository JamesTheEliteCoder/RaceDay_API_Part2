namespace PROG_Part2_RaceDay_API.DTOs;

public class EventResultResponse
{
    public int ResultId { get; set; }

    public int EnrolmentId { get; set; }

    public int ParticipantId { get; set; }

    public string ParticipantName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public int FinishingPosition { get; set; }

    public TimeOnly FinishTime { get; set; }
}