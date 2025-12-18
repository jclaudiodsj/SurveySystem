using SurveySystem.Domain.Shared;

namespace SurveySystem.Domain.Surveys
{
    public sealed class Submission : Entity
    {
        public Guid SurveyId { get; private set; }
        public DateTimeOffset SubmittedAt { get; private set; }

        private readonly List<Answer> _answers;
        public IReadOnlyList<Answer> Answers => _answers.AsReadOnly();

        private Submission(Guid SurveyId, DateTimeOffset SubmittedAt)//, List<Answer> Answers) 
        {
            this.SurveyId = SurveyId;
            this.SubmittedAt = SubmittedAt;

            _answers = new List<Answer>();
            //_answers.AddRange(Answers);
        }

        public static Submission Create(Guid SurveyId, DateTimeOffset SubmittedAt, IEnumerable<Answer> Answers)
        {
            if (SurveyId == Guid.Empty) 
                throw new DomainException("SurveyId cannot be empty.");

            var listAnswers = Answers?.ToList() ?? throw new DomainException("Answers cannot be null.");

            if (listAnswers.Count() == 0)
                throw new DomainException("A submission must contain at least one answer.");

            // Regra: múltipla escolha simples => no máximo 1 resposta por pergunta no mesmo envio.
            var duplicatedQuestion = listAnswers
                .GroupBy(a => a.QuestionId)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicatedQuestion is not null)
                throw new DomainException($"Duplicate answers for QuestionId '{duplicatedQuestion.Key}' are not allowed.");

            return new Submission(SurveyId, SubmittedAt);//, listAnswers.ToList());            
        }
    }
}
