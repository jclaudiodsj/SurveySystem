namespace SurveySystem.Api.Dtos.Surveys.Responses
{
    public class OptionResultResponse
    {
        public string Text { get; set; } = string.Empty;
        public int TotalVotes { get; set; } = new int();
        public double PercentualVotes { get; set; } = new double();
    }
}
