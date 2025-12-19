namespace SurveySystem.Api.Dtos.Surveys
{
    public class QuestionResponse
    {
        public string Text { get; set; } = String.Empty;
        public int Order { get; set; } = 0;
        public List<OptionResponse> Options { get; set; } = new List<OptionResponse>();        
    }
}
