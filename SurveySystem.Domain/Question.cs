using SurveySystem.Domain.Shared;

namespace SurveySystem.Domain
{
    public class Question : ValueObject
    {
        public string Text { get; private set; }

        public int Order { get; private set; }

        private readonly List<Option> _options;
        public IReadOnlyList<Option> Options => _options.AsReadOnly();

        private Question()
        {
            this.Text = string.Empty;
            this.Order = 0;
            this._options = new List<Option>();
        }

        public static Question Create(string Text, int Order, List<string> Options)
        {
            ValidateText(Text);
            ValidateOrder(Order);
            ValidateOptions(Options);

            var q = new Question();

            q.Text = Text;
            q.Order = Order;

            for (int i = 0; i < Options.Count; i++)
                q._options.Add(Option.Create(Options[i], i));

            return q;
        }

        private static void ValidateText(string Text)
        {
            if (string.IsNullOrWhiteSpace(Text))
                throw new DomainException("Question text cannot be null or empty.");
        }

        private static void ValidateOrder(int Order)
        {
            if (Order < 0)
                throw new DomainException("Order cannot be negative.");
        }

        private static void ValidateOptions(List<string> options)
        {
            if (options == null || options.Count < 2)
                throw new DomainException("A question must have at least two options.");

            if (options.Count != options.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                throw new DomainException("Option texts must be unique.");
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Text;
        }
    }
}
