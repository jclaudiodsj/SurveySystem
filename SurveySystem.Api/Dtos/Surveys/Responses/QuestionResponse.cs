namespace SurveySystem.Api.Dtos.Surveys.Responses
{
    public class QuestionResponse
    {
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
        public List<OptionResponse> Options { get; set; } = new List<OptionResponse>();        
    }
}
