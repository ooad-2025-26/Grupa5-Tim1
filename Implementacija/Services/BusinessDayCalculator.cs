namespace bibliotecha.Services
{
    public static class BusinessDayCalculator
    {
        public static DateOnly AddBusinessDays(DateOnly startDate, int numberOfDays)
        {
            if (numberOfDays < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfDays));
            }

            var result = startDate;
            var addedDays = 0;

            while (addedDays < numberOfDays)
            {
                result = result.AddDays(1);

                if (result.DayOfWeek is not DayOfWeek.Saturday
                    and not DayOfWeek.Sunday)
                {
                    addedDays++;
                }
            }

            return result;
        }
    }
}