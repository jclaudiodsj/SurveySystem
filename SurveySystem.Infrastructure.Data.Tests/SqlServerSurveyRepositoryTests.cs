using Microsoft.EntityFrameworkCore;
using SurveySystem.Domain.Surveys;
using SurveySystem.Infrastructure.Data.Repositories;

namespace SurveySystem.Infrastructure.Data.Tests
{
    // A classe de testes para o SqlServerSurveyRepository.
    // Utiliza IClassFixture para compartilhar o setup do DbContext em memória entre os testes,
    // garantindo que cada teste comece com um estado limpo.
    public class SqlServerSurveyRepositoryTests : IClassFixture<SurveySystemRepositoryTestFixture>
    {
        private readonly SurveySystemRepositoryTestFixture _fixture;
        private readonly SqlServerSurveyRepository _repositorySurvey;        
        private readonly SurveyDbContext _context;

        public SqlServerSurveyRepositoryTests(SurveySystemRepositoryTestFixture fixture)
        {
            _fixture = fixture;
            // Para cada teste, obtemos um novo contexto e repositório para garantir isolamento.
            // O fixture garante que o contexto seja resetado (limpo) antes de cada teste.

            _fixture.InitializeAsync().Wait();

            _context = _fixture.DbContext;
            _repositorySurvey = new SqlServerSurveyRepository(_context);
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
        public async Task AddAsyncSurveyShouldAddSuccessfully()
        {
            var survey = CreateTestSurvey("Customer Satisfaction", "Survey about customer satisfaction", "How satisfied are you with our service?");
            // Act
            await _repositorySurvey.Add(survey);
            await _context.SaveChangesAsync();

            // Assert
            var retrievedSurvey = await _context.Surveys
                                                .Include(s => s.Period)
                                                .Include(s => s.Questions)
                                                .FirstOrDefaultAsync(s => s.Id == survey.Id);

            Assert.NotNull(retrievedSurvey);
            Assert.Equal(survey.Title, retrievedSurvey!.Title);
            Assert.Equal(survey.Description, retrievedSurvey!.Description);
            Assert.Single(retrievedSurvey!.Questions);
            Assert.Equal(survey.Questions[0].Text, retrievedSurvey!.Questions[0].Text);
            Assert.Equal(survey.Questions[0].Options.Count, retrievedSurvey!.Questions[0].Options.Count);
            Assert.Equal(survey.Questions[0].Options[0], retrievedSurvey!.Questions[0].Options[0]);
        }

        [Fact]
        public async Task UpdateAsyncSurveyShouldUpdateSuccessfully()
        {
            // Arrange
            var survey = CreateTestSurvey("Employee Engagement", "Survey about employee engagement", "How engaged do you feel at work?");
            await _repositorySurvey.Add(survey);
            await _context.SaveChangesAsync();
            // Act
            survey.UpdateDetails("Updated Title", "Updated Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(15));
            await _repositorySurvey.Update(survey);
            await _context.SaveChangesAsync();
            // Assert
            var updatedSurvey = await _context.Surveys.FindAsync(survey.Id);
            Assert.NotNull(updatedSurvey);
            Assert.Equal("Updated Title", updatedSurvey.Title);
            Assert.Equal("Updated Description", updatedSurvey.Description);
        }

        [Fact]
        public async Task DeleteAsyncSurveyShouldDeleteSuccessfully()
        {
            // Arrange
            var survey = CreateTestSurvey("Product Feedback", "Survey about product feedback", "What do you think about our product?");
            await _repositorySurvey.Add(survey);
            await _context.SaveChangesAsync();
            // Act
            await _repositorySurvey.Delete(survey.Id);
            await _context.SaveChangesAsync();
            // Assert
            var deletedSurvey = await _context.Surveys.FindAsync(survey.Id);
            Assert.Null(deletedSurvey);
        }

        [Fact]
        public async Task GetAllAsyncShouldReturnAllSurveys()
        {
            // Arrange
            var survey1 = CreateTestSurvey("Survey 1", "Description 1", "Question 1?");
            var survey2 = CreateTestSurvey("Survey 2", "Description 2", "Question 2?");
            await _repositorySurvey.Add(survey1);
            await _repositorySurvey.Add(survey2);
            await _context.SaveChangesAsync();
            // Act
            var surveys = await _repositorySurvey.GetAll();
            // Assert
            Assert.NotNull(surveys);
            Assert.True(surveys.Count >= 2);
            Assert.Contains(surveys, s => s.Id == survey1.Id);
            Assert.Contains(surveys, s => s.Id == survey2.Id);
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnSurvey()
        {
            // Arrange
            var survey = CreateTestSurvey("Market Research", "Survey about market research", "What are your buying habits?");
            await _repositorySurvey.Add(survey);
            await _context.SaveChangesAsync();
            // Act
            var retrievedSurvey = await _repositorySurvey.GetById(survey.Id);
            // Assert
            Assert.NotNull(retrievedSurvey);
            Assert.Equal(survey.Title, retrievedSurvey!.Title);
            Assert.Equal(survey.Description, retrievedSurvey!.Description);
        }

        [Fact]
        public async Task SurveyExistsAfterAddition()
        {
            // Arrange
            var survey = CreateTestSurvey("Brand Awareness", "Survey about brand awareness", "How familiar are you with our brand?");
            // Act
            await _repositorySurvey.Add(survey);
            await _context.SaveChangesAsync();
            var exists = await _context.Surveys.AnyAsync(s => s.Id == survey.Id);
            // Assert
            Assert.True(exists);
        }
    }
}
