namespace SurveySystem.Api.Dtos.Surveys.Requests
{
    public class SurveyPeriodRequest
    {
        public DateTimeOffset StartDate { get; set; } = new DateTime();
        public DateTimeOffset EndDate { get; set; } = new DateTime(); 
    }
}
