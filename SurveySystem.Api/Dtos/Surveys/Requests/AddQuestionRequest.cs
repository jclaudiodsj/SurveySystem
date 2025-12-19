namespace SurveySystem.Api.Dtos.Surveys.Requests
{
    public class AddQuestionRequest
    {
        public string Text { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
    }
}
