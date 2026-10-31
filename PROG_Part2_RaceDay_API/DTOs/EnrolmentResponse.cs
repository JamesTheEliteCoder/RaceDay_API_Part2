namespace PROG_Part2_RaceDay_API.DTOs;

public class EnrolmentResponse
{
    // Identifies the enrolment record.
    public int EnrolmentId { get; set; }

    // Identifies the participant and provides their display information.
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string ParticipantEmail { get; set; } = string.Empty;

    // Identifies the event and selected category.
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    // Records when the participant enrolled and their current status.
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}