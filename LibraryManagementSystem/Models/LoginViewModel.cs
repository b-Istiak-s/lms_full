// Person 2: Login, registration, user account, roles, and security support.
using System.ComponentModel.DataAnnotations;
namespace LibraryManagementSystem.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
