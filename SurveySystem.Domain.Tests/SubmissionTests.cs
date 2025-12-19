using SurveySystem.Domain.Surveys;

namespace SurveySystem.Domain.Tests
{
    public class SubmissionTests
    {
        [Fact]
        public void Create_ValidParameters_ShouldCreateSubmission()
        {
            // Arrange
            Guid surveyId = Guid.NewGuid();
            DateTimeOffset submittedAt = DateTimeOffset.UtcNow;
            var answers = new List<Answer>
            {
                Answer.Create("Question 1", "Option 1"),
                Answer.Create("Question 2", "Option 1")
            };
            // Act
            var submission = Submission.Create(surveyId, submittedAt, answers);
            // Assert
            Assert.Equal(surveyId, submission.SurveyId);
            Assert.Equal(submittedAt, submission.SubmittedAt);
            Assert.Equal(2, submission.Answers.Count);
        }
    }
}