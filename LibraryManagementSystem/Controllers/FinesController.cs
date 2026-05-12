// Person 3: Admin/librarian module for managing library records and reports.
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FinesController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public FinesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            dbContext = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var list = await dbContext.Fines.Include(f => f.BorrowTransaction).ThenInclude(b => b.Book).ToListAsync();
            var userIds = list.Select(f => f.BorrowTransaction?.UserId).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            var users = await userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            ViewBag.Users = users;
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var fine = await dbContext.Fines.FindAsync(id);
            if (fine != null)
            {
                fine.IsPaid = true;
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
