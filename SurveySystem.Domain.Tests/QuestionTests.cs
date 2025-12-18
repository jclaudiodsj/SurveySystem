using SurveySystem.Domain.Shared;

namespace SurveySystem.Domain.Tests
{
    public class QuestionTests
    {
        [Fact]
        public void Create_ValidParameters_ShouldCreateQuestion()
        {
            // Arrange
            string text = "What is your favorite color?";
            int order = 1;
            List<string> options = new List<string> { "Red", "Blue", "Green" };
            // Act
            var question = Question.Create(text, order, options);
            // Assert
            Assert.Equal(text, question.Text);
            Assert.Equal(order, question.Order);
            Assert.Equal(3, question.Options.Count);
            Assert.Equal("Red", question.Options[0].Text);
            Assert.Equal("Blue", question.Options[1].Text);
            Assert.Equal("Green", question.Options[2].Text);
        }

        [Fact]
        public void Create_InvalidText_ShouldThrowDomainException()
        {
            // Arrange
            string invalidText = "   ";
            int order = 1;
            List<string> options = new List<string> { "Option 1", "Option 2" };
            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => Question.Create(invalidText, order, options));
            Assert.Equal("Question text cannot be null or empty.", exception.Message);
        }

        [Fact]
        public void Create_NegativeOrder_ShouldThrowDomainException()
        {
            // Arrange
            string text = "What is your favorite color?";
            int negativeOrder = -1;
            List<string> options = new List<string> { "Option 1", "Option 2" };
            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => Question.Create(text, negativeOrder, options));
            Assert.Equal("Order cannot be negative.", exception.Message);
        }

        [Fact]
        public void Create_InsufficientOptions_ShouldThrowDomainException()
        {
            // Arrange
            string text = "What is your favorite color?";
            int order = 1;
            List<string> insufficientOptions = new List<string> { "Only one option" };
            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => Question.Create(text, order, insufficientOptions));
            Assert.Equal("A question must have at least two options.", exception.Message);
        }

        [Fact]
        public void Create_DuplicateOptions_ShouldThrowDomainException()
        {
            // Arrange
            string text = "What is your favorite color?";
            int order = 1;
            List<string> duplicateOptions = new List<string> { "Red", "Blue", "red" };
            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => Question.Create(text, order, duplicateOptions));
            Assert.Equal("Option texts must be unique.", exception.Message);
        }
    }
}
