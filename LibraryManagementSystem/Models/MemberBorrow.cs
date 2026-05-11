namespace LibraryManagementSystem.Models
{
    // Person 4: Model used for the member borrow form and borrow history page.
    public class MemberBorrow
    {
        public string MemberName { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}
