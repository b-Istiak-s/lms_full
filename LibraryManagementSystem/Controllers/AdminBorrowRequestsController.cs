// Person 3: Admin page for viewing borrowed books, returns, and fines.
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBorrowRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminBorrowRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var query = _context.BorrowTransactions
                .Include(t => t.Book)
                .Include(t => t.Fine)
                .AsQueryable();

            if (status == "borrowed")
            {
                query = query.Where(t => !t.IsReturned);
            }
            else if (status == "returned")
            {
                query = query.Where(t => t.IsReturned);
            }

            var transactions = await query
                .OrderByDescending(t => t.BorrowDate)
                .ToListAsync();

            var memberIds = transactions.Select(t => t.UserId).Distinct().ToList();
            var members = await _userManager.Users
                .Where(u => memberIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            ViewBag.Members = members;
            ViewBag.StatusFilter = status ?? "all";
            return View(transactions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id, DateTime borrowDate, DateTime returnDate)
        {
            var tx = await _context.BorrowTransactions
                .Include(t => t.Book)
                .Include(t => t.Fine)
                .FirstOrDefaultAsync(t => t.BorrowTransactionId == id);

            if (tx == null)
            {
                return NotFound();
            }

            if (returnDate.Date < borrowDate.Date)
            {
                TempData["AdminBorrowMessage"] = "Return date cannot be before the borrowing date.";
                return RedirectToAction(nameof(Index));
            }

            var rule = await _context.BorrowRules.FirstOrDefaultAsync();
            var loanDays = rule?.LoanDays ?? 14;
            var finePerDay = rule?.FinePerDay ?? 2.00m;

            tx.BorrowDate = borrowDate.Date;
            tx.DueDate = borrowDate.Date.AddDays(loanDays);
            tx.ReturnDate = returnDate.Date;
            tx.IsReturned = true;

            if (tx.Book != null)
            {
                tx.Book.IsAvailable = true;
            }

            var daysLate = Math.Max(0, (returnDate.Date - tx.DueDate.Date).Days);
            var fineAmount = daysLate * finePerDay;

            if (tx.Fine == null)
            {
                _context.Fines.Add(new Fine
                {
                    BorrowTransactionId = tx.BorrowTransactionId,
                    Amount = fineAmount,
                    IsPaid = fineAmount == 0
                });
            }
            else
            {
                tx.Fine.Amount = fineAmount;
                tx.Fine.IsPaid = fineAmount == 0 ? true : tx.Fine.IsPaid;
            }

            await _context.SaveChangesAsync();
            TempData["AdminBorrowMessage"] = $"Book returned. Fine automatically calculated as ${fineAmount:0.00}.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFinePaid(int id)
        {
            var fine = await _context.Fines.FindAsync(id);
            if (fine == null)
            {
                return NotFound();
            }

            fine.IsPaid = true;
            await _context.SaveChangesAsync();
            TempData["AdminBorrowMessage"] = "Fine marked as paid.";
            return RedirectToAction(nameof(Index));
        }
    }
}
