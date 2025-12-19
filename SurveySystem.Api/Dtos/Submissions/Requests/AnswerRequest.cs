using System.ComponentModel.DataAnnotations;

namespace SurveySystem.Api.Dtos.Submissions.Requests
{
    public class AnswerRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(500)]
        public string QuestionText { get; set; } = string.Empty;
        [Required]
        [MinLength(1)]
        [MaxLength(500)]
        public string OptionText { get; set; } = string.Empty;
    }
}
