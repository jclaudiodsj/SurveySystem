using SurveySystem.Domain.Shared;
using SurveySystem.Domain.Surveys;

namespace SurveySystem.Domain.Tests.Surveys
{
    public class OptionTests
    {
        [Fact]
        public void Create_ValidParameters_ShouldCreateOption()
        {
            // Arrange
            string text = "Option 1";
            int order = 1;
            // Act
            var option = Option.Create(text, order);
            // Assert
            Assert.Equal(text, option.Text);
            Assert.Equal(order, option.Order);
        }

        [Fact]
        public void Create_InvalidText_ShouldThrowDomainException()
        {
            // Arrange
            string invalidText = "   ";
            int order = 1;
            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => Option.Create(invalidText, order));
            Assert.Equal("Option text cannot be null or empty.", exception.Message);
        }

        [Fact]
        public void Create_NegativeOrder_ShouldThrowDomainException()
        {
            // Arrange
            string text = "Option 1";
            int negativeOrder = -1;
            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => Option.Create(text, negativeOrder));
            Assert.Equal("Order cannot be negative.", exception.Message);
        }
    }
}
