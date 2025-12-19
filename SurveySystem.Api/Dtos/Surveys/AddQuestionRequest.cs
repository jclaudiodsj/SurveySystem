
namespace SurveySystem.Api.Dtos.Surveys
{
    public class AddQuestionRequest
    {
        public string Text { get; set; } = String.Empty;
        public List<string> Options { get; set; } = new List<string>();
    }
}
