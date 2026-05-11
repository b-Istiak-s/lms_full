namespace LibraryManagementSystem.Models
{
    // Person 4: Model used for the member fine calculator page.
    public class FineCalculator
    {
        public string MemberName { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public int DaysLate { get; set; }
        public double Amount { get; set; }
    }
}
