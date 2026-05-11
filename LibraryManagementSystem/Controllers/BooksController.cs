// Person 3: Admin/librarian module for managing library records and reports.
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public BooksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            dbContext = context;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string? status, string? isbn)
        {
            var query = dbContext.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .AsQueryable();

            if (status == "available")
            {
                query = query.Where(b => b.IsAvailable);
            }
            else if (status == "unavailable")
            {
                query = query.Where(b => !b.IsAvailable);
            }

            if (!string.IsNullOrWhiteSpace(isbn))
            {
                var cleanIsbn = isbn.Trim();
                query = query.Where(b => b.ISBN.Contains(cleanIsbn));
                ViewBag.SearchIsbn = cleanIsbn;
            }

            ViewBag.StatusFilter = status ?? "all";
            ViewBag.TotalBooks = await dbContext.Books.CountAsync();
            ViewBag.AvailableBooks = await dbContext.Books.CountAsync(b => b.IsAvailable);
            ViewBag.UnavailableBooks = await dbContext.Books.CountAsync(b => !b.IsAvailable);

            var bookList = await query
                .OrderBy(b => b.Title)
                .ToListAsync();

            var activeBorrowings = await dbContext.BorrowTransactions
                .Include(t => t.Book)
                .Where(t => !t.IsReturned)
                .ToListAsync();

            var memberIds = activeBorrowings.Select(t => t.UserId).Distinct().ToList();
            var members = await userManager.Users
                .Where(u => memberIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            ViewBag.ActiveBorrowers = activeBorrowings
                .GroupBy(t => t.BookId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var t = g.First();
                        return members.ContainsKey(t.UserId)
                            ? $"{members[t.UserId].FullName ?? members[t.UserId].Email} ({members[t.UserId].Email})"
                            : t.UserId;
                    });

            return View(bookList);
        }

        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, IFormFile? coverImage, string? newAuthorName, string? newGenreName)
        {
            newAuthorName = string.IsNullOrWhiteSpace(newAuthorName) ? null : newAuthorName.Trim();
            newGenreName = string.IsNullOrWhiteSpace(newGenreName) ? null : newGenreName.Trim();

            if (book.AuthorId <= 0 && newAuthorName is not null)
            {
                book.AuthorId = await FindOrCreateAuthorIdAsync(newAuthorName);
            }

            if (book.GenreId <= 0 && newGenreName is not null)
            {
                book.GenreId = await FindOrCreateGenreIdAsync(newGenreName);
            }

            if (!await dbContext.Authors.AnyAsync(a => a.AuthorId == book.AuthorId))
            {
                ModelState.AddModelError(nameof(book.AuthorId), "Please select a valid author.");
            }

            if (!await dbContext.Genres.AnyAsync(g => g.GenreId == book.GenreId))
            {
                ModelState.AddModelError(nameof(book.GenreId), "Please select a valid genre.");
            }

            if (ModelState.IsValid)
            {
                book.IsAvailable = true;
                book.CoverImagePath = await SaveCoverImageAsync(coverImage, book.CoverImagePath);
                dbContext.Add(book);
                await dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(book.AuthorId, book.GenreId, newAuthorName, newGenreName);
            return View(book);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var book = await dbContext.Books.FindAsync(id);
            if (book == null) return NotFound();

            PopulateDropdowns(book.AuthorId, book.GenreId);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book, IFormFile? coverImage, string? newAuthorName, string? newGenreName)
        {
            if (id != book.BookId) return NotFound();

            var existingBook = await dbContext.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (existingBook == null)
            {
                return NotFound();
            }

            newAuthorName = string.IsNullOrWhiteSpace(newAuthorName) ? null : newAuthorName.Trim();
            newGenreName = string.IsNullOrWhiteSpace(newGenreName) ? null : newGenreName.Trim();

            if (book.AuthorId <= 0 && newAuthorName is not null)
            {
                book.AuthorId = await FindOrCreateAuthorIdAsync(newAuthorName);
            }

            if (book.GenreId <= 0 && newGenreName is not null)
            {
                book.GenreId = await FindOrCreateGenreIdAsync(newGenreName);
            }

            if (!await dbContext.Authors.AnyAsync(a => a.AuthorId == book.AuthorId))
            {
                ModelState.AddModelError(nameof(book.AuthorId), "Please select a valid author.");
            }

            if (!await dbContext.Genres.AnyAsync(g => g.GenreId == book.GenreId))
            {
                ModelState.AddModelError(nameof(book.GenreId), "Please select a valid genre.");
            }

            if (ModelState.IsValid)
            {
                book.CoverImagePath = await SaveCoverImageAsync(coverImage, existingBook.CoverImagePath);
                dbContext.Update(book);
                await dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            book.CoverImagePath = existingBook.CoverImagePath;
            PopulateDropdowns(book.AuthorId, book.GenreId, newAuthorName, newGenreName);
            return View(book);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var book = await dbContext.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(b => b.BookId == id);
            if (book == null) return NotFound();
            return View(book);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var book = await dbContext.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await dbContext.Books.FindAsync(id);
            if (book != null)
            {
                dbContext.Books.Remove(book);
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns(int? selectedAuthorId = null, int? selectedGenreId = null, string? newAuthorName = null, string? newGenreName = null)
        {
            var authorList = dbContext.Authors
                .OrderBy(a => a.Name)
                .ToList();

            var genreList = dbContext.Genres
                .OrderBy(g => g.Name)
                .ToList();

            ViewBag.AuthorOptions = new SelectList(authorList, nameof(Author.AuthorId), nameof(Author.Name), selectedAuthorId);
            ViewBag.GenreOptions = new SelectList(genreList, nameof(Genre.GenreId), nameof(Genre.Name), selectedGenreId);
            ViewBag.NewAuthorName = newAuthorName;
            ViewBag.NewGenreName = newGenreName;
        }

        private async Task<int> FindOrCreateAuthorIdAsync(string authorName)
        {
            var foundAuthor = await dbContext.Authors
                .FirstOrDefaultAsync(a => a.Name.ToLower() == authorName.ToLower());

            if (foundAuthor is not null)
            {
                return foundAuthor.AuthorId;
            }

            var createdAuthor = new Author { Name = authorName };
            dbContext.Authors.Add(createdAuthor);
            await dbContext.SaveChangesAsync();
            return createdAuthor.AuthorId;
        }

        private async Task<int> FindOrCreateGenreIdAsync(string genreName)
        {
            var foundGenre = await dbContext.Genres
                .FirstOrDefaultAsync(g => g.Name.ToLower() == genreName.ToLower());

            if (foundGenre is not null)
            {
                return foundGenre.GenreId;
            }

            var createdGenre = new Genre { Name = genreName };
            dbContext.Genres.Add(createdGenre);
            await dbContext.SaveChangesAsync();
            return createdGenre.GenreId;
        }

        private async Task<string?> SaveCoverImageAsync(IFormFile? coverImage, string? currentCoverPath)
        {
            if (coverImage == null || coverImage.Length == 0)
            {
                return currentCoverPath;
            }

            var uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images", "books", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(coverImage.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await coverImage.CopyToAsync(stream);
            }

            DeleteUploadedCoverIfManaged(currentCoverPath);
            return $"/images/books/uploads/{fileName}";
        }

        private void DeleteUploadedCoverIfManaged(string? coverPath)
        {
            if (string.IsNullOrWhiteSpace(coverPath) || !coverPath.StartsWith("/images/books/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var relativePath = coverPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(webHostEnvironment.WebRootPath, relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
