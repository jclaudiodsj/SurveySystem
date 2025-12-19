namespace SurveySystem.Api.Dtos.Submissions.Requests
{
    public class AnswerRequest
    {
        public string QuestionText { get; set; } = string.Empty;
        public string OptionText { get; set; } = string.Empty;
    }
}
