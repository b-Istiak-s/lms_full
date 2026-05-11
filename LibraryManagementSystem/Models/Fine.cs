// Person 1: Database design, model classes, relationships, DbContext, and migration support.
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Fine
    {
        public int FineId { get; set; }

        public int BorrowTransactionId { get; set; }
        public BorrowTransaction? BorrowTransaction { get; set; }

        public decimal Amount { get; set; }

        public bool IsPaid { get; set; }
    }
}