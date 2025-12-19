using SurveySystem.Domain.Shared;

namespace SurveySystem.Domain.Submissions
{
    public class Answer : ValueObject
    {
        public string QuestionText { get; private set; }
        public string OptionText { get; private set; }

        private Answer(string QuestionText, string OptionText)
        {
            this.QuestionText = QuestionText;
            this.OptionText = OptionText;
        }

        public static Answer Create(string questionText, string optionText)
        {
            if (string.IsNullOrEmpty(questionText))
                throw new DomainException("QuestionText cannot be empty.");

            if (string.IsNullOrEmpty(optionText))
                throw new DomainException("OptionText cannot be empty.");

            return new Answer(questionText, optionText);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return QuestionText;
            yield return OptionText;
        }
    }
}
