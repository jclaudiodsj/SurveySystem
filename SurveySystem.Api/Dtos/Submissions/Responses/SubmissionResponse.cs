namespace SurveySystem.Api.Dtos.Submissions.Responses
{
    public class SubmissionResponse
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
        public List<AnswerResponse> Answers { get; set; } = new List<AnswerResponse>();
    }
}
