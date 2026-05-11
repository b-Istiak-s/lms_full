// Person 1: Database design, model classes, relationships, DbContext, and migration support.
namespace LibraryManagementSystem.Models
{
    public class LibraryProfile
    {
        public int LibraryProfileId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string? OperatingHours { get; set; }

        public string? ContactDetails { get; set; }
    }
}
