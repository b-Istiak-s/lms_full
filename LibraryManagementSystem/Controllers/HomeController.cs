using System.Diagnostics;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ApplicationDbContext dbContext;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewBag.AvailableCount = await dbContext.Books.CountAsync(b => b.IsAvailable);
            ViewBag.TotalBooks = await dbContext.Books.CountAsync();
            ViewBag.BorrowedCount = await dbContext.BorrowTransactions.CountAsync(t => !t.IsReturned);

            ViewBag.NewArrivals = await dbContext.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .OrderByDescending(b => b.BookId)
                .Take(4)
                .ToListAsync();

            ViewBag.AvailableBooks = await dbContext.Books
                .Include(b => b.Author)
                .Where(b => b.IsAvailable)
                .OrderBy(b => b.Title)
                .Take(4)
                .ToListAsync();

            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
