namespace SurveySystem.Api.Dtos.Surveys.Requests
{
    public class UpdateSurveyRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SurveyPeriodRequest SurveyPeriod { get; set; } = new SurveyPeriodRequest();
    }
}
