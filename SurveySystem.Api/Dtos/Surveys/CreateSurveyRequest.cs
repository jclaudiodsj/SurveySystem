namespace SurveySystem.Api.Dtos.Surveys
{
    public class CreateSurveyRequest
    {
        public string Title { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public SurveyPeriodRequest SurveyPeriod { get; set; } = new SurveyPeriodRequest();
        public List<AddQuestionRequest> Questions { get; set; } = new List<AddQuestionRequest>();
    }
}
