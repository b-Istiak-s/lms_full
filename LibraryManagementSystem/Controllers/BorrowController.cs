// Person 3: Admin/librarian module for managing library records and reports.
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using System;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BorrowController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public BorrowController(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await dbContext.BorrowTransactions
                .Include(b => b.Book)
                .Include(b => b.Fine)
                .ToListAsync();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookId, string userName, DateTime? borrowDate, DateTime? dueDate)
        {
            var book = await dbContext.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                TempData["BorrowError"] = "User name is required.";
                return RedirectToAction("Index", "Books");
            }

            if (!book.IsAvailable)
            {
                TempData["BorrowError"] = "This book is not available for borrowing.";
                return RedirectToAction("Index", "Books");
            }

            var startDate = borrowDate?.Date ?? DateTime.Now.Date;
            var endDate = dueDate?.Date ?? startDate.AddDays(14);
            if (endDate < startDate)
            {
                TempData["BorrowError"] = "Due date cannot be before borrow date.";
                return RedirectToAction("Index", "Books");
            }

            var tx = new BorrowTransaction
            {
                BookId = bookId,
                UserId = userName.Trim(),
                BorrowDate = startDate,
                DueDate = endDate,
                IsReturned = false
            };

            book.IsAvailable = false;
            dbContext.BorrowTransactions.Add(tx);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Books");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id, DateTime borrowDate, DateTime returnDate)
        {
            var tx = await dbContext.BorrowTransactions
                .Include(b => b.Book)
                .Include(b => b.Fine)
                .FirstOrDefaultAsync(b => b.BorrowTransactionId == id);
            if (tx == null) return NotFound();

            if (returnDate.Date < borrowDate.Date)
            {
                TempData["FineError"] = "Return date cannot be before borrow date.";
                return RedirectToAction(nameof(Index));
            }

            var rule = await dbContext.BorrowRules.FirstOrDefaultAsync();
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
            var existingFine = await dbContext.Fines.FirstOrDefaultAsync(f => f.BorrowTransactionId == tx.BorrowTransactionId);
            if (existingFine == null)
            {
                dbContext.Fines.Add(new Fine
                {
                    BorrowTransactionId = tx.BorrowTransactionId,
                    Amount = fineAmount,
                    IsPaid = fineAmount == 0
                });
            }
            else
            {
                existingFine.Amount = fineAmount;
                existingFine.IsPaid = fineAmount == 0 ? true : existingFine.IsPaid;
            }

            await dbContext.SaveChangesAsync();
            TempData["FineError"] = $"Book returned. Fine automatically calculated as ${fineAmount:0.00}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFine(int borrowTransactionId, decimal amount, bool isPaid)
        {
            var tx = await dbContext.BorrowTransactions.FindAsync(borrowTransactionId);
            if (tx == null)
            {
                return NotFound();
            }

            if (amount < 0)
            {
                TempData["FineError"] = "Fine amount cannot be negative.";
                return RedirectToAction(nameof(Index));
            }

            var existingFine = await dbContext.Fines.FirstOrDefaultAsync(f => f.BorrowTransactionId == borrowTransactionId);
            if (existingFine == null)
            {
                dbContext.Fines.Add(new Fine
                {
                    BorrowTransactionId = borrowTransactionId,
                    Amount = amount,
                    IsPaid = isPaid
                });
            }
            else
            {
                existingFine.Amount = amount;
                existingFine.IsPaid = isPaid;
            }

            await dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
