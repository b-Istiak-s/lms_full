namespace LibraryManagementSystem.Models
{
    // Person 4: Model used for the member-facing browse books and book details pages.
    public class MemberBook
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string CoverImagePath { get; set; } = string.Empty;
    }
}
