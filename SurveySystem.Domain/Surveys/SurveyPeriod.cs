using SurveySystem.Domain.Shared;

namespace SurveySystem.Domain.Surveys
{
    public class SurveyPeriod : ValueObject
    {
        public DateTimeOffset StartDate { get; private set; }

        public DateTimeOffset EndDate { get; private set; }

        private SurveyPeriod(DateTimeOffset StartDate, DateTimeOffset EndDate)
        {
            this.StartDate = StartDate;
            this.EndDate = EndDate;
        }

        public static SurveyPeriod Create(DateTimeOffset StartDate, DateTimeOffset EndDate)
        {
            ValidatePeriod(StartDate, EndDate);

            return new SurveyPeriod(StartDate, EndDate);
        }

        private static void ValidatePeriod(DateTimeOffset StartDate, DateTimeOffset EndDate)
        {
            if (EndDate <= StartDate)
                throw new DomainException("End date must be later than start date.");
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StartDate;
            yield return EndDate;
        }
    }
}
