using SurveySystem.Domain.Shared;

namespace SurveySystem.Domain
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
                throw new ArgumentException("Option text cannot be null or empty.", nameof(Text));
        }

        private static void ValidateOrder(int Order)
        {
            if (Order < 0)
                throw new ArgumentOutOfRangeException(nameof(Order), "Order cannot be negative.");
        }        

        public void UpdateText(string newText)
        {
            ValidateText(Text);

            Text = newText;
        }

        public void UpdateOrder(int newOrder)
        {
            ValidateOrder(Order);

            Order = newOrder;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Text;
        }
    }
}
