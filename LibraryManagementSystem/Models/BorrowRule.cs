// Person 1: Database design, model classes, relationships, DbContext, and migration support.
namespace LibraryManagementSystem.Models
{
    public class BorrowRule
    {
        public int BorrowRuleId { get; set; }

        public int MaxBooks { get; set; }

        public int LoanDays { get; set; }

        public decimal FinePerDay { get; set; }
    }
}