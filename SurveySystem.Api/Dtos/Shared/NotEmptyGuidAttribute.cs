using System.ComponentModel.DataAnnotations;

namespace SurveySystem.Api.Dtos.Shared
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class NotEmptyGuidAttribute : ValidationAttribute
    {
        public NotEmptyGuidAttribute() : base("The {0} field must not be empty.") { }

        public override bool IsValid(object? value)
        {
            if (value is null) return false;
            return value is Guid g && g != Guid.Empty;
        }
    }
}
