using FluentAssertions;
using SurveySystem.Domain.Shared;
using SurveySystem.Domain.Surveys;

namespace SurveySystem.Domain.Tests.Surveys
{
    public class SurveyTests
    {
        [Fact]
        public void CreateSurvey_WithValidParameters_ShouldCreateSurvey()
        {
            // Arrange
            string title = "Customer Satisfaction Survey";
            string description = "A survey to gauge customer satisfaction levels.";
            DateTimeOffset startDate = DateTimeOffset.Now;
            DateTimeOffset endDate = startDate.AddDays(30);
            // Act
            var survey = Survey.Create(title, description, startDate, endDate);
            // Assert
            Assert.NotNull(survey);
            Assert.Equal(title, survey.Title);
            Assert.Equal(description, survey.Description);
            Assert.Equal(startDate, survey.Period.StartDate);
            Assert.Equal(endDate, survey.Period.EndDate);
            Assert.Equal(SurveyStatus.Draft, survey.Status);
        }

        [Fact]
        public void CreateSurvey_WithEmptyTitle_ShouldThrowArgumentException()
        {
            // Arrange
            string title = "";
            string description = "A survey description.";
            DateTimeOffset startDate = DateTimeOffset.Now;
            DateTimeOffset endDate = startDate.AddDays(30);
            // Act & Assert
            Action act = () => Survey.Create(title, description, startDate, endDate);
            act.Should()
                .Throw<DomainException>()
                .WithMessage("Title cannot be null or empty.");
        }

        [Fact]
        public void CreateSurvey_WithEndDateBeforeStartDate_ShouldThrowArgumentException()
        {
            // Arrange
            string title = "Survey Title";
            string description = "A survey description.";
            DateTimeOffset startDate = DateTimeOffset.Now;
            DateTimeOffset endDate = startDate.AddDays(-1);
            // Act & Assert
            Action act = () => Survey.Create(title, description, startDate, endDate);
            act.Should()
                .Throw<DomainException>()
                .WithMessage("End date must be later than start date.");
        }

        [Fact]
        public void UpdateSurveyDetails_InDraftStatus_ShouldUpdateDetails()
        {
            // Arrange
            var survey = Survey.Create("Initial Title", "Initial Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            string newTitle = "Updated Title";
            string newDescription = "Updated Description";
            DateTimeOffset newStartDate = DateTimeOffset.Now.AddDays(1);
            DateTimeOffset newEndDate = newStartDate.AddDays(20);
            // Act
            survey.UpdateDetails(newTitle, newDescription, newStartDate, newEndDate);
            // Assert
            Assert.Equal(newTitle, survey.Title);
            Assert.Equal(newDescription, survey.Description);
            Assert.Equal(newStartDate, survey.Period.StartDate);
            Assert.Equal(newEndDate, survey.Period.EndDate);
        }

        [Fact]
        public void UpdateSurveyDetails_NotInDraftStatus_ShouldThrowDomainException()
        {
            // Arrange
            var survey = Survey.Create("Initial Title", "Initial Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            string questionText = "What is your favorite color?";
            List<string> options = new List<string> { "Red", "Blue", "Green" };
            survey.AddQuestion(questionText, options);
            survey.Publish(); // Change status to Published
            string newTitle = "Updated Title";
            string newDescription = "Updated Description";
            DateTimeOffset newStartDate = DateTimeOffset.Now.AddDays(1);
            DateTimeOffset newEndDate = newStartDate.AddDays(20);
            // Act & Assert
            Action act = () => survey.UpdateDetails(newTitle, newDescription, newStartDate, newEndDate);
            act.Should()
                .Throw<DomainException>()
                .WithMessage("Only surveys in Draft status can be updated.");
        }

        [Fact]
        public void UpdateSurveyDetails_WithEmptyTitle_ShouldThrowDomainException()
        {
            // Arrange
            var survey = Survey.Create("Initial Title", "Initial Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            string newTitle = "";
            string newDescription = "Updated Description";
            DateTimeOffset newStartDate = DateTimeOffset.Now.AddDays(1);
            DateTimeOffset newEndDate = newStartDate.AddDays(20);
            // Act & Assert
            Action act = () => survey.UpdateDetails(newTitle, newDescription, newStartDate, newEndDate);
            act.Should()
                .Throw<DomainException>()
                .WithMessage("Title cannot be null or empty.");
        }

        [Fact]
        public void UpdateSurveyDetails_WithEndDateBeforeStartDate_ShouldThrowDomainException()
        {
            // Arrange
            var survey = Survey.Create("Initial Title", "Initial Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            string newTitle = "Updated Title";
            string newDescription = "Updated Description";
            DateTimeOffset newStartDate = DateTimeOffset.Now.AddDays(10);
            DateTimeOffset newEndDate = newStartDate.AddDays(-1);
            // Act & Assert
            Action act = () => survey.UpdateDetails(newTitle, newDescription, newStartDate, newEndDate);
            act.Should()
                .Throw<DomainException>()
                .WithMessage("End date must be later than start date.");
        }

        [Fact]
        public void AddQuestion_WithUniqueText_ShouldAddQuestion()
        {
            // Arrange
            var survey = Survey.Create("Survey Title", "Survey Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            string questionText = "What is your favorite color?";
            List<string> options = new List<string> { "Red", "Blue", "Green" };
            // Act
            survey.AddQuestion(questionText, options);
            // Assert
            Assert.Single(survey.Questions);
            Assert.Equal(questionText, survey.Questions[0].Text);

            survey.Questions[0].Options.Should().HaveCount(3);

            for (int i = 0; i < options.Count; i++)
                survey.Questions[0].Options.Should().ContainSingle(o => o.Text == options[i] && o.Order == i);
        }

        [Fact]
        public void AddQuestion_InStatusPublished_ShouldThrowDomainException()
        {
            // Arrange
            var survey = Survey.Create("Survey Title", "Survey Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            string questionText = "What is your favorite color?";
            List<string> options = new List<string> { "Red", "Blue", "Green" };
            survey.AddQuestion(questionText, options);
            survey.Publish(); // Change status to Published
            string questionText2 = "What is your favorite weather?";
            List<string> options2 = new List<string> { "Sunny", "Rainy" };
            // Act & Assert
            Action act = () => survey.AddQuestion(questionText2, options2);
            act.Should()
                .Throw<DomainException>()
                .WithMessage("Only surveys in Draft status can be modified.");
        }

        [Fact]
        public void AddQuestion_WithDuplicateText_ShouldThrowDomainException()
        {
            // Arrange
            var survey = Survey.Create("Survey Title", "Survey Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            string questionText = "What is your favorite color?";
            List<string> options = new List<string> { "Red", "Blue", "Green" };
            survey.AddQuestion(questionText, options);
            // Act & Assert
            Action act = () => survey.AddQuestion(questionText, options);
            act.Should()
                .Throw<DomainException>()
                .WithMessage("A question with the same text already exists in the survey.");
        }

        [Fact]
        public void RemoveQuestion_WithValidIndex_ShouldRemoveQuestionAndReorder()
        {
            // Arrange
            var survey = Survey.Create("Survey Title", "Survey Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            survey.AddQuestion("Question 1", new List<string> { "A", "B" });
            survey.AddQuestion("Question 2", new List<string> { "C", "D" });
            survey.AddQuestion("Question 3", new List<string> { "E", "F" });
            // Act
            survey.RemoveQuestion(1); // Remove "Question 2"
            // Assert
            Assert.Equal(2, survey.Questions.Count);
            Assert.Equal("Question 1", survey.Questions[0].Text);
            Assert.Equal(0, survey.Questions[0].Order);
            Assert.Equal("Question 3", survey.Questions[1].Text);
            Assert.Equal(1, survey.Questions[1].Order);
        }

        [Fact]
        public void RemoveQuestion_WithInvalidIndex_ShouldThrowDomainException()
        {
            // Arrange
            var survey = Survey.Create("Survey Title", "Survey Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            survey.AddQuestion("Question 1", new List<string> { "A", "B" });
            // Act & Assert
            Action act = () => survey.RemoveQuestion(5); // Invalid index
            act.Should()
                .Throw<DomainException>()
                .WithMessage("Invalid question index.");
        }

        [Fact]
        public void PublishSurvey_InDraftStatus_ShouldChangeStatusToPublished()
        {
            // Arrange
            var survey = Survey.Create("Survey Title", "Survey Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            survey.AddQuestion("Question 1", new List<string> { "A", "B" });
            // Act
            survey.Publish();
            // Assert
            Assert.Equal(SurveyStatus.Published, survey.Status);
        }

        [Fact]
        public void PublishSurvey_NotInDraftStatus_ShouldThrowDomainException()
        {
            // Arrange
            var survey = Survey.Create("Survey Title", "Survey Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            survey.AddQuestion("Question 1", new List<string> { "A", "B" });
            survey.Publish();
            // Act & Assert
            Action act = () => survey.Publish();
            act.Should()
                .Throw<DomainException>()
                .WithMessage("Only surveys in Draft status can be published.");
        }

        [Fact]
        public void PublishSurvey_WithoutQuestions_ShouldThrowDomainException()
        {
            // Arrange
            var survey = Survey.Create("Survey Title", "Survey Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            // Act & Assert
            Action act = () => survey.Publish();
            act.Should()
                .Throw<DomainException>()
                .WithMessage("Cannot publish a survey with no questions.");
        }

        [Fact]
        public void CloseSurvey_Published() {
            // Arrange
            var survey = Survey.Create("Survey Title", "Survey Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            survey.AddQuestion("Question 1", new List<string> { "A", "B" });
            survey.Publish();
            // Act
            survey.Close();
            // Assert
            Assert.Equal(SurveyStatus.Closed, survey.Status);
        }

        [Fact]
        public void CloseSurvey_NotPublished() {
            // Arrange
            var survey = Survey.Create("Survey Title", "Survey Description", DateTimeOffset.Now, DateTimeOffset.Now.AddDays(10));
            // Act & Assert
            Action act = () => survey.Close();
            act.Should()
                .Throw<DomainException>()
                .WithMessage("Only surveys in Published status can be closed.");
        }
    }
}
