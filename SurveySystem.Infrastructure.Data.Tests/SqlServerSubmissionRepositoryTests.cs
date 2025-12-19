using Microsoft.EntityFrameworkCore;
using SurveySystem.Domain.Submissions;
using SurveySystem.Domain.Surveys;
using SurveySystem.Infrastructure.Data.Repositories;

namespace SurveySystem.Infrastructure.Data.Tests
{
    // A classe de testes para o SqlServerSubmissionRepository.
    // Utiliza IClassFixture para compartilhar o setup do DbContext em memória entre os testes,
    // garantindo que cada teste comece com um estado limpo.
    public class SqlServerSubmissionRepositoryTests : IClassFixture<SurveySystemRepositoryTestFixture>
    {
        private readonly SurveySystemRepositoryTestFixture _fixture;        
        private readonly SqlServerSubmissionRepository _repositorySubmission;
        private readonly SurveySystemDbContext _context;

        public SqlServerSubmissionRepositoryTests(SurveySystemRepositoryTestFixture fixture)
        {
            _fixture = fixture;
            // Para cada teste, obtemos um novo contexto e repositório para garantir isolamento.
            // O fixture garante que o contexto seja resetado (limpo) antes de cada teste.

            _fixture.InitializeAsync().Wait();

            _context = _fixture.DbContext;            
            _repositorySubmission = new SqlServerSubmissionRepository(_context);
        }

        // Helper para criar uma enquete de teste com dados padrão.
        private Survey CreateTestSurvey(
            string title,
            string description,
            string questionText)
        {
            var survey = Survey.Create(title, description, DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));

            List<string> options = new List<string> { "Option 1", "Option 2", "Option 3" };

            survey.AddQuestion(questionText, options);

            return survey;
        }

        [Fact]
        public async Task AddAsyncSubmissionShouldAddSuccessfully()
        {
            // Arrange
            var survey = CreateTestSurvey("Customer Satisfaction", "Survey about customer satisfaction", "How satisfied are you with our service?");
            // Persist survey first because Submission references SurveyId (foreign key).
            await _context.Surveys.AddAsync(survey);
            await _context.SaveChangesAsync();

            var questionText = survey.Questions[0].Text;
            var answers = new List<Answer>
            {
                Answer.Create(questionText, "Option 1")
            };

            var submission = Submission.Create(survey.Id, DateTimeOffset.UtcNow, answers);

            // Act
            await _repositorySubmission.Add(submission);
            await _context.SaveChangesAsync();

            // Assert
            var retrieved = await _context.Submissions
                                          .Include(s => s.Answers)
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync(s => s.Id == submission.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(submission.SurveyId, retrieved.SurveyId);
            Assert.Equal(submission.SubmittedAt, retrieved.SubmittedAt);
            Assert.Single(retrieved.Answers);
            Assert.Equal(answers[0].QuestionText, retrieved.Answers[0].QuestionText);
            Assert.Equal(answers[0].OptionText, retrieved.Answers[0].OptionText);
        }

        [Fact]
        public async Task GetSubmissionByIdAsyncShouldReturnSubmission()
        {
            // Arrange
            var survey = CreateTestSurvey("Employee Feedback", "Survey about employee feedback", "How do you rate your work environment?");
            await _context.Surveys.AddAsync(survey);
            await _context.SaveChangesAsync();
            var questionText = survey.Questions[0].Text;
            var answers = new List<Answer>
            {
                Answer.Create(questionText, "Option 2")
            };
            var submission = Submission.Create(survey.Id, DateTimeOffset.UtcNow, answers);
            await _repositorySubmission.Add(submission);
            await _context.SaveChangesAsync();
            // Act
            var retrieved = await _repositorySubmission.GetById(submission.Id);
            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(submission.SurveyId, retrieved.SurveyId);
            Assert.Equal(submission.SubmittedAt, retrieved.SubmittedAt);
            Assert.Single(retrieved.Answers);
            Assert.Equal(answers[0].QuestionText, retrieved.Answers[0].QuestionText);
            Assert.Equal(answers[0].OptionText, retrieved.Answers[0].OptionText);
        }

        [Fact]
        public async Task GetSubmissionsBySurveyIdAsyncShouldReturnSubmissions()
        {
            // Arrange
            var survey = CreateTestSurvey("Product Usage", "Survey about product usage", "How often do you use our product?");
            await _context.Surveys.AddAsync(survey);
            await _context.SaveChangesAsync();
            var questionText = survey.Questions[0].Text;
            var submission1 = Submission.Create(survey.Id, DateTimeOffset.UtcNow, new List<Answer>
            {
                Answer.Create(questionText, "Option 1")
            });
            var submission2 = Submission.Create(survey.Id, DateTimeOffset.UtcNow, new List<Answer>
            {
                Answer.Create(questionText, "Option 2")
            });
            await _repositorySubmission.Add(submission1);
            await _repositorySubmission.Add(submission2);
            await _context.SaveChangesAsync();
            // Act
            var submissions = await _repositorySubmission.GetBySurveyId(survey.Id);
            // Assert
            Assert.NotNull(submissions);
            Assert.True(submissions.Count >= 2);
        }
    }
}
