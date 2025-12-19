namespace SurveySystem.Api.Dtos.Surveys.Responses
{
    public class QuestionResultResponse
    {
        public string Text { get; set; } = string.Empty;        
        public List<OptionResultResponse> Options { get; set; } = new List<OptionResultResponse>();
        public int TotalVotes { get; set; } = new int();
    }
}
