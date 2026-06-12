namespace bibliotecha.Services
{
    public class NotificationSettings
    {
        public bool Enabled { get; set; } = true;
        public int CheckIntervalMinutes { get; set; } = 5;
        public int LoanDurationDays { get; set; } = 14;
        public int ReservationPickupBusinessDays { get; set; } = 3;
    }
}
