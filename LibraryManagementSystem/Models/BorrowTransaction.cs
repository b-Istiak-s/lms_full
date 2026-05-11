// Person 1: Database design, model classes, relationships, DbContext, and migration support.
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class BorrowTransaction
    {
        public int BorrowTransactionId { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public bool IsReturned { get; set; }

        public Fine? Fine { get; set; }
    }
}