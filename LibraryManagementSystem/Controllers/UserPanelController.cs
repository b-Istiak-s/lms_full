// Person 2 and Person 4: Shows the logged-in member/user panel after login.
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class UserPanelController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserPanelController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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

            return View();
        }
    }
}
