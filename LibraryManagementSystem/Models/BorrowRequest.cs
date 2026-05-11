using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    // Person 3/4: A member request for borrowing a book before admin approval.
    public class BorrowRequest
    {
        public int RequestId { get; set; }

        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;

        [Required]
        public string MemberId { get; set; } = string.Empty;

        public string MemberName { get; set; } = string.Empty;
        public string MemberEmail { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; } = DateTime.Today;
        public DateTime? BorrowDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Returned
        public int? BorrowTransactionId { get; set; }
        public string? AdminNote { get; set; }
    }
}
