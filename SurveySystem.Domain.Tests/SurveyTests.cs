namespace SurveySystem.Domain.Tests
{
    public class SurveyTests
    {
        [Fact]
        public void Create_ShouldInitializeSurveyWithGivenParameters()
        {
            // Arrange
            string title = "Customer Satisfaction Survey";
            string description = "A survey to gauge customer satisfaction levels.";
            DateTimeOffset startDate = new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset endDate = new DateTimeOffset(2024, 7, 31, 23, 59, 59, TimeSpan.Zero);
            // Act
            Survey survey = Survey.Create(title, description, startDate, endDate);
            // Assert
            Assert.Equal(title, survey.Title);
            Assert.Equal(description, survey.Description);
            Assert.Equal(SurveyStatus.Draft, survey.Status);
            Assert.Equal(startDate, survey.Period.StartDate);
            Assert.Equal(endDate, survey.Period.EndDate);
        }
    }
}
