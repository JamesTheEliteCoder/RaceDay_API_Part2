namespace RaceDay_MVC_Part3.Models
{
    // Represents an individual race result returned by the API.
    public class ParticipantResultViewModel
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
}