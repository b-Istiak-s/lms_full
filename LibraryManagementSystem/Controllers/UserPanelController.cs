// Person 2 and Person 4: Shows the logged-in member/user panel after login.
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class UserPanelController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserPanelController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;
            ViewBag.Email = user.Email;
            ViewBag.MemberId = user.Id;
            ViewBag.Roles = roles.Any() ? string.Join(", ", roles) : "Member";

            // Load user's feedbacks including book info and admin replies
            var feedbacks = await _context.Feedbacks
                .Where(f => f.UserId == user.Id)
                .Include(f => f.Book)
                .OrderByDescending(f => f.FeedbackId)
                .ToListAsync();

            ViewBag.UserFeedbacks = feedbacks;

            return View();
        }
    }
}
