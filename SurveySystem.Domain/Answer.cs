using SurveySystem.Domain.Shared;

namespace SurveySystem.Domain
{
    public class Answer : ValueObject
    {
        public Guid QuestionId { get; private set; }
        public Guid OptionId { get; private set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return QuestionId;
            yield return OptionId;
        }
    }
}
