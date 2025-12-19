using SurveySystem.Api.Dtos.Shared;
using System.ComponentModel.DataAnnotations;

namespace SurveySystem.Api.Dtos.Submissions.Requests
{
    public class CreateSubmissionRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one answer is required.")]
        public List<AnswerRequest> Answers { get; set; } = new List<AnswerRequest>();
    }
}
