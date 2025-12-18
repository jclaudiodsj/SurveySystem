using SurveySystem.Domain.Shared;

namespace SurveySystem.Domain
{
    public class Survey : Entity
    {
        public string Title { get; private set; }

        public string? Description { get; private set; }

        public SurveyStatus Status { get; private set; }

        public SurveyPeriod Period { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? UpdatedAt { get; private set; }

        public DateTimeOffset? PublishedAt { get; private set; }

        public DateTimeOffset? ClosedAt { get; private set; }

        private readonly List<Question> _Questions;
        public IReadOnlyList<Question> Questions => _Questions.AsReadOnly();

        private Survey(string Title, string Description, DateTimeOffset StartDate, DateTimeOffset EndDate)
        {
            this.Title = Title;
            this.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            this.Status = SurveyStatus.Draft;
            this.Period = SurveyPeriod.Create(StartDate, EndDate);
            this.CreatedAt = DateTimeOffset.UtcNow;
            this.UpdatedAt = DateTimeOffset.UtcNow;
            this.PublishedAt = null;
            this.ClosedAt = null;
            this._Questions = new List<Question>();
        }

        public static Survey Create(string Title, string Description, DateTimeOffset StartDate, DateTimeOffset EndDate)
        {
            ValidateTitle(Title);

            return new Survey(Title, Description, StartDate, EndDate);
        }

        private static void ValidateTitle(string Title)
        {
            if (string.IsNullOrWhiteSpace(Title))
                throw new DomainException("Title cannot be null or empty.");
        }

        public void UpdateDetails(string Title, string Description, DateTimeOffset StartDate, DateTimeOffset EndDate)
        {
            if (Status != SurveyStatus.Draft)
                throw new DomainException("Only surveys in Draft status can be updated.");

            ValidateTitle(Title);            

            this.Title = Title;
            this.Description = Description;
            this.Period = SurveyPeriod.Create(StartDate, EndDate);
            this.UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void AddQuestion(string Text, List<string> Options)
        {
            if (Status != SurveyStatus.Draft)
                throw new DomainException("Only surveys in Draft status can be modified.");

            if (_Questions.Any(q => q.Text.Equals(Text, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("A question with the same text already exists in the survey.");

            this.UpdatedAt = DateTimeOffset.UtcNow;
            this._Questions.Add(Question.Create(Text, _Questions.Count, Options));
        }

        public void RemoveQuestion(int questionIndex)
        {
            if (Status != SurveyStatus.Draft)
                throw new DomainException("Only surveys in Draft status can be modified.");

            if (questionIndex < 0 || questionIndex >= _Questions.Count)
                throw new DomainException("Invalid question index.");

            this.UpdatedAt = DateTimeOffset.UtcNow;
            _Questions.RemoveAt(questionIndex);
            
            ReorderQuestions();
        }

        private void ReorderQuestions()
        {   
            for (int i = 0; i < _Questions.Count; i++)
                _Questions[i] = Question.Create(_Questions[i].Text, i, _Questions[i].Options.Select(o => o.Text).ToList());
        }

        public void Publish()
        {
            if (Status != SurveyStatus.Draft)
                throw new DomainException("Only surveys in Draft status can be published.");

            if (_Questions.Count == 0)
                throw new DomainException("Cannot publish a survey with no questions.");

            PublishedAt = DateTimeOffset.UtcNow;
            Status = SurveyStatus.Published;
        }

        public void Close()
        {
            if (Status != SurveyStatus.Published)
                throw new DomainException("Only surveys in Published status can be closed.");

            ClosedAt = DateTimeOffset.UtcNow;
            Status = SurveyStatus.Closed;
        }
    }
}
