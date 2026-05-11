// Person 1: Database design, model classes, relationships, DbContext, and migration support.
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}