namespace LibraryManagementSystem.Models
{
    // Person 4: Model used for the member book feedback and rating page.
    public class BookFeedback
    {
        public string MemberName { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
