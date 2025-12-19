namespace SurveySystem.Api.Dtos.Surveys.Responses
{
    public class SurveyPeriodResponse
    {
        public DateTimeOffset StartDate { get; set; } = new DateTime();
        public DateTimeOffset EndDate { get; set; } = new DateTime(); 
    }
}
