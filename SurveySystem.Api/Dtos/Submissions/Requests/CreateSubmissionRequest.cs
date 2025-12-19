namespace SurveySystem.Api.Dtos.Submissions.Requests
{
    public class CreateSubmissionRequest
    {
        public Guid SurveyId { get; set; }
        public List<AnswerRequest> Answers { get; set; } = new List<AnswerRequest>();
    }
}
