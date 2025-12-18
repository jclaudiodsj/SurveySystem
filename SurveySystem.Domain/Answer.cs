using SurveySystem.Domain.Shared;

namespace SurveySystem.Domain
{
    public class Answer : ValueObject
    {
        public Guid QuestionId { get; private set; }
        public Guid OptionId { get; private set; }

        private Answer(Guid QuestionId, Guid OptionId)
        {
            this.QuestionId = QuestionId;
            this.OptionId = OptionId;
        }

        public static Answer Create(Guid questionId, Guid optionId)
        {
            if (questionId == Guid.Empty)
                throw new DomainException("QuestionId cannot be empty.");

            if (optionId == Guid.Empty)
                throw new DomainException("OptionId cannot be empty.");

            return new Answer(questionId, optionId);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return QuestionId;
            yield return OptionId;
        }
    }
}
