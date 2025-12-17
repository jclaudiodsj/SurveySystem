using SurveySystem.Domain.Shared;

namespace SurveySystem.Domain
{
    public class Question : ValueObject
    {
        public string Text { get; private set; }

        public int Order { get; private set; }

        private readonly List<Option> _options;
        public IReadOnlyList<Option> Options => _options.AsReadOnly();

        private Question(string Text, int Order)
        {
            this.Text = Text;
            this.Order = Order;
            this._options = new List<Option>();
        }

        public static Question Create(string Text, int Order)
        {
            ValidateText(Text);
            ValidateOrder(Order);

            return new Question(Text, Order);
        }
        
        private static void ValidateText(string Text)
        {
            if (string.IsNullOrWhiteSpace(Text))
                throw new ArgumentException("Question text cannot be null or empty.", nameof(Text));
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

        public void AddOption(string Text)
        {
            if(_options.Any(o => o.Text.Equals(Text, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("An option with the same text already exists for this question.");

            _options.Add(Option.Create(Text, _options.Count));
        }

        public void UpdateOptionText(int optionIndex, string newText)
        {
            if (optionIndex < 0 || optionIndex >= _options.Count)
                throw new ArgumentOutOfRangeException(nameof(optionIndex), "Option index is out of range.");

            if (_options.Where((o, i) => i != optionIndex)
                        .Any(o => o.Text.Equals(newText, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("An option with the same text already exists for this question.");

            _options[optionIndex].UpdateText(newText);
        }

        public void RemoveOption(int optionIndex)
        {
            if (optionIndex < 0 || optionIndex >= _options.Count)
                throw new ArgumentOutOfRangeException(nameof(optionIndex), "Option index is out of range.");

            _options.RemoveAt(optionIndex);
            
            ReorderOptions(optionIndex);
        }

        private void ReorderOptions(int optionIndex)
        {
            for (int i = optionIndex; i < _options.Count; i++)
                _options[i].UpdateOrder(i);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Text;
        }
    }
}
