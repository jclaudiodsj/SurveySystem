using SurveySystem.Domain.Shared;

namespace SurveySystem.Domain
{
    public class Survey : Entity
    {
        public string Title { get; private set; }

        public string Description { get; private set; }

        public SurveyStatus Status { get; private set; }

        public SurveyPeriod Period { get; private set; }

        private readonly List<Question> _questions;
        public IReadOnlyList<Question> Questions => _questions.AsReadOnly();

        private Survey(string Title, string Description, DateTimeOffset StartDate, DateTimeOffset EndDate)
        {
            this.Title = Title;
            this.Description = Description;
            this.Period = SurveyPeriod.Create(StartDate, EndDate);
            this._questions = new List<Question>();
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

        private static void ValidateDescription(string description)
        {

        }

        public void UpdateDetails(string Title, string Description)
        {
            ValidateTitle(Title);
            ValidateDescription(Description);

            if(Status != SurveyStatus.Draft)
                throw new InvalidOperationException("Only surveys in Draft status can be updated.");

            this.Title = Title;
            this.Description = Description;
        }

        public void AddQuestion(string Text)
        {
            if (Status != SurveyStatus.Draft)
                throw new InvalidOperationException("Only surveys in Draft status can be modified.");

            if (_questions.Any(q => q.Text.Equals(Text, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A question with the same text already exists in the survey.");

            _questions.Add(Question.Create(Text, _questions.Count));
        }

        public void UpdateQuestionText(int questionIndex, string newText)
        {
            if (questionIndex < 0 || questionIndex >= _questions.Count)
                throw new ArgumentOutOfRangeException(nameof(questionIndex), "Invalid question index.");

            if (_questions.Where((q, i) => i != questionIndex)
                        .Any(q => q.Text.Equals(newText, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A question with the same text already exists in the survey.");

            _questions[questionIndex].UpdateText(newText);
        }

        public void AddOptionToQuestion(int questionIndex, string optionText)
        {
            if (questionIndex < 0 || questionIndex >= _questions.Count)
                throw new ArgumentOutOfRangeException(nameof(questionIndex), "Invalid question index.");

            _questions[questionIndex].AddOption(optionText);
        }

        public void UpdateOptionTextInQuestion(int questionIndex, int optionIndex, string newText)
        {
            if (questionIndex < 0 || questionIndex >= _questions.Count)
                throw new ArgumentOutOfRangeException(nameof(questionIndex), "Invalid question index.");

            _questions[questionIndex].UpdateOptionText(optionIndex, newText);
        }

        public void RemoveOptionFromQuestion(int questionIndex, int optionIndex)
        {
            if (questionIndex < 0 || questionIndex >= _questions.Count)
                throw new ArgumentOutOfRangeException(nameof(questionIndex), "Invalid question index.");

            _questions[questionIndex].RemoveOption(optionIndex);
        }

        public void RemoverQuestion(int questionIndex)
        {
            if (questionIndex < 0 || questionIndex >= _questions.Count)
                throw new ArgumentOutOfRangeException(nameof(questionIndex), "Invalid question index.");

            _questions.RemoveAt(questionIndex);
            
            ReorderQuestions();
        }

        private void ReorderQuestions()
        {   
            for (int i = 0; i < _questions.Count; i++)
                _questions[i].UpdateOrder(i);
        }

        public void Publish()
        {
            if (Status != SurveyStatus.Draft)
                throw new InvalidOperationException("Only surveys in Draft status can be published.");

            if (_questions.Count == 0)
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
