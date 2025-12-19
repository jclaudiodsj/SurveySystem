using System.ComponentModel.DataAnnotations;

namespace SurveySystem.Api.Dtos.Surveys.Requests
{
    public class SurveyPeriodRequest
    {
        [Required]
        public DateTimeOffset StartDate { get; set; }
        [Required]
        public DateTimeOffset EndDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate == default)
                yield return new ValidationResult("StartDate is required.", new[] { nameof(StartDate) });

            if (EndDate == default)
                yield return new ValidationResult("EndDate is required.", new[] { nameof(EndDate) });

            if (StartDate != default && EndDate != default && EndDate <= StartDate)
                yield return new ValidationResult("EndDate must be greater than StartDate.", new[] { nameof(EndDate) });
        }
    }
}
