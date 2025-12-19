using SurveySystem.Domain.Shared;

namespace SurveySystem.Domain.Surveys
{
    public class Option : ValueObject
    {
        public string Text { get; private set; }

        public int Order { get; private set; }

        private Option(string Text, int Order)
        {
            this.Text = Text;
            this.Order = Order;
        }

        public static Option Create(string Text, int Order)
        {
            ValidateText(Text);
            ValidateOrder(Order);

            return new Option(Text, Order);
        }

        private static void ValidateText(string Text)
        {
            if (string.IsNullOrWhiteSpace(Text))
                throw new DomainException("Option text cannot be null or empty.");
        }

        private static void ValidateOrder(int Order)
        {
            if (Order < 0)
                throw new DomainException("Order cannot be negative.");
        }        

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Text;
        }
    }
}
