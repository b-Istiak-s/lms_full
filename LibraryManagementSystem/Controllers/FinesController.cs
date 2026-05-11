// Person 3: Admin/librarian module for managing library records and reports.
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FinesController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public FinesController(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await dbContext.Fines.Include(f => f.BorrowTransaction).ThenInclude(b => b.Book).ToListAsync();
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
