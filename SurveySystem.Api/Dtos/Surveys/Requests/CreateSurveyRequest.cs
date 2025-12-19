using System.ComponentModel.DataAnnotations;

namespace SurveySystem.Api.Dtos.Surveys.Requests
{
    public class CreateSurveyRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        [Required]
        public SurveyPeriodRequest SurveyPeriod { get; set; } = new SurveyPeriodRequest();
        public List<AddQuestionRequest> Questions { get; set; } = new List<AddQuestionRequest>();
    }
}
