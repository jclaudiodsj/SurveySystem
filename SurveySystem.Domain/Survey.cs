using SurveySystem.Domain.Shared;

namespace SurveySystem.Domain
{
    public class Survey : Entity
    {
        public string Title { get; private set; }

        public string Description { get; private set; }

        public SurveyStatus Status { get; private set; }

        public SurveyPeriod Period { get; private set; }

        private readonly List<Question> _Questions;
        public IReadOnlyList<Question> Questions => _Questions.AsReadOnly();

        private Survey(string Title, string Description, DateTimeOffset StartDate, DateTimeOffset EndDate)
        {
            this.Title = Title;
            this.Description = Description;
            this.Period = SurveyPeriod.Create(StartDate, EndDate);
            this._Questions = new List<Question>();
        }

        public static Survey Create(string Title, string Description, DateTimeOffset StartDate, DateTimeOffset EndDate)
        {
            ValidateTitle(Title);
            ValidateDescription(Description);

            return new Survey(Title, Description, StartDate, EndDate);
        }

        private static void ValidateTitle(string Title)
        {
            if (string.IsNullOrWhiteSpace(Title))
                throw new ArgumentException("Title cannot be null or empty.", nameof(Title));
        }

        private static void ValidateDescription(string Description)
        {

        }

        public void UpdateDetails(string Title, string Description, DateTimeOffset StartDate, DateTimeOffset EndDate)
        {
            if (Status != SurveyStatus.Draft)
                throw new InvalidOperationException("Only surveys in Draft status can be updated.");

            ValidateTitle(Title);
            ValidateDescription(Description);

            this.Title = Title;
            this.Description = Description;
            this.Period.UpdatePeriod(StartDate, EndDate);
        }

        public void AddQuestion(string Text, List<string> Options)
        {
            if (Status != SurveyStatus.Draft)
                throw new InvalidOperationException("Only surveys in Draft status can be modified.");

            if (_Questions.Any(q => q.Text.Equals(Text, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A question with the same text already exists in the survey.");

            _Questions.Add(Question.Create(Text, _Questions.Count, Options));
        }

        public void RemoverQuestion(int questionIndex)
        {
            if (questionIndex < 0 || questionIndex >= _Questions.Count)
                throw new ArgumentOutOfRangeException(nameof(questionIndex), "Invalid question index.");

            _Questions.RemoveAt(questionIndex);
            
            ReorderQuestions();
        }

        private void ReorderQuestions()
        {   
            for (int i = 0; i < _Questions.Count; i++)
                _Questions[i].UpdateOrder(i);
        }

        public void Publish()
        {
            if (Status != SurveyStatus.Draft)
                throw new InvalidOperationException("Only surveys in Draft status can be published.");

            if (_Questions.Count == 0)
                throw new InvalidOperationException("Cannot publish a survey with no questions.");

            Status = SurveyStatus.Published;
        }

        public void Close()
        {
            if (Status != SurveyStatus.Published)
                throw new InvalidOperationException("Only surveys in Published status can be closed.");

            Status = SurveyStatus.Closed;
        }
    }
}
