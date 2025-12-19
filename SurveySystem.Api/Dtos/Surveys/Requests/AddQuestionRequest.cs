using System.ComponentModel.DataAnnotations;

namespace SurveySystem.Api.Dtos.Surveys.Requests
{
    public class AddQuestionRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(500)]
        public string Text { get; set; } = string.Empty;
        [Required]
        [MinLength(2, ErrorMessage = "A question must have at least two options.")]
        public List<string> Options { get; set; } = new List<string>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Options is null || Options.Count < 2)
                yield break;

            // opções vazias
            var emptyIndexes = Options
                .Select((v, i) => new { v, i })
                .Where(x => string.IsNullOrWhiteSpace(x.v))
                .Select(x => x.i)
                .ToList();

            if (emptyIndexes.Count > 0)
                yield return new ValidationResult("Options cannot contain null/empty values.", new[] { nameof(Options) });

            // duplicadas (case-insensitive)
            var normalized = Options
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o.Trim())
                .ToList();

            if (normalized.Count != normalized.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                yield return new ValidationResult("Option texts must be unique (case-insensitive).", new[] { nameof(Options) });
        }
    }
}
