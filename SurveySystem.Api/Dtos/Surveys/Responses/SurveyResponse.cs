using SurveySystem.Domain.Surveys;

namespace SurveySystem.Api.Dtos.Surveys.Responses
{
    public class SurveyResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;   

        public string Description { get; set; } = string.Empty;

        public SurveyStatus Status { get; set; } = SurveyStatus.Draft;

        public SurveyPeriodResponse Period { get; set; } = new SurveyPeriodResponse();

        public DateTimeOffset CreatedAt { get; set; } = new DateTimeOffset();

        public DateTimeOffset? UpdatedAt { get; set; } = null;

        public DateTimeOffset? PublishedAt { get; set; } = null;

        public DateTimeOffset? ClosedAt { get; set; } = null;

        public List<QuestionResponse> Question { get; set; } = new List<QuestionResponse>();        
    }
}
