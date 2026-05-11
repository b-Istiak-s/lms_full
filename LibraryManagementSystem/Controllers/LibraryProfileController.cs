// Person 3: Admin/librarian module for managing library records and reports.
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LibraryProfileController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public LibraryProfileController(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public async Task<IActionResult> Edit()
        {
            var profile = await dbContext.LibraryProfiles.FirstOrDefaultAsync();
            var rule = await dbContext.BorrowRules.FirstOrDefaultAsync();

            if (profile == null)
            {
                profile = new LibraryProfile
                {
                    Name = "KOI Online Library",
                    Location = "Sydney",
                    OperatingHours = "9:00 AM - 5:00 PM",
                    ContactDetails = "library@example.com"
                };
            }

            if (rule == null)
            {
                rule = new BorrowRule
                {
                    MaxBooks = 5,
                    LoanDays = 14,
                    FinePerDay = 2.00m
                };
            }

            ViewBag.BorrowRule = rule;
            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LibraryProfile profile, int maxBooks, int loanDays, decimal finePerDay)
        {
            if (maxBooks < 1)
            {
                ModelState.AddModelError("MaxBooks", "Maximum books must be at least 1.");
            }

            if (loanDays < 1)
            {
                ModelState.AddModelError("LoanDays", "Loan days must be at least 1.");
            }

            if (finePerDay < 0)
            {
                ModelState.AddModelError("FinePerDay", "Fine per day cannot be negative.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.BorrowRule = new BorrowRule
                {
                    MaxBooks = maxBooks,
                    LoanDays = loanDays,
                    FinePerDay = finePerDay
                };
                return View(profile);
            }

            var existingProfile = await dbContext.LibraryProfiles.FirstOrDefaultAsync();
            if (existingProfile == null)
            {
                dbContext.LibraryProfiles.Add(profile);
            }
            else
            {
                existingProfile.Name = profile.Name;
                existingProfile.Location = profile.Location;
                existingProfile.OperatingHours = profile.OperatingHours;
                existingProfile.ContactDetails = profile.ContactDetails;
                dbContext.LibraryProfiles.Update(existingProfile);
            }

            var existingRule = await dbContext.BorrowRules.FirstOrDefaultAsync();
            if (existingRule == null)
            {
                dbContext.BorrowRules.Add(new BorrowRule
                {
                    MaxBooks = maxBooks,
                    LoanDays = loanDays,
                    FinePerDay = finePerDay
                });
            }
            else
            {
                existingRule.MaxBooks = maxBooks;
                existingRule.LoanDays = loanDays;
                existingRule.FinePerDay = finePerDay;
                dbContext.BorrowRules.Update(existingRule);
            }

            await dbContext.SaveChangesAsync();
            TempData["ProfileSaved"] = "Library profile and borrowing rules saved successfully.";
            return RedirectToAction(nameof(Edit));
        }
    }
}
