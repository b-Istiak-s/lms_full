using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Shared request store used by member borrow requests and admin approvals.
builder.Services.AddSingleton<BorrowRequestStore>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Person 2: Configure user login, registration, password rules, and roles.
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole>()
.AddDefaultTokenProviders()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Person 2: Set login and access-denied paths for protected pages.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Ensure the demo book-cover images exist even after copying/extracting the project.
EnsureBookCoverFiles(app.Environment.WebRootPath);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Keep the local database in sync with the current migrations before seeding roles/users.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    var identityTablesExist = await TableExistsAsync(dbContext, "AspNetRoles");
    var libraryProfileColumnsExist = await ColumnExistsAsync(dbContext, "LibraryProfiles", "ContactDetails");
    var returnDateColumnExists = await ColumnExistsAsync(dbContext, "BorrowTransactions", "ReturnDate");

    if (!identityTablesExist || !libraryProfileColumnsExist || !returnDateColumnExists)
    {
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }
}

// Person 2: Seed default Admin and Member roles plus one admin test account.
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Member" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "admin@library.com";
    var adminPassword = "Admin123!";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Library Admin",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}



// Seed sample authors, genres, books and default borrowing rules for demo use.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var authorNames = new[]
    {
        "George Orwell", "Robert C. Martin", "Thomas Connolly", "Ian Griffiths", "Rui Peres",
        "Ben Frain", "Mark J. Price", "Andrew Hunt", "David Thomas", "Jon Duckett",
        "Martin Fowler", "Steve Krug"
    };

    foreach (var authorName in authorNames)
    {
        if (!await dbContext.Authors.AnyAsync(a => a.Name == authorName))
        {
            dbContext.Authors.Add(new Author { Name = authorName });
        }
    }
    await dbContext.SaveChangesAsync();

    var genreNames = new[]
    {
        "Political Fiction", "Programming", "Information Technology", "Database", "Web Development",
        "Cybersecurity", "Software Engineering", "Project Management", "User Experience", "Cloud Computing"
    };

    foreach (var genreName in genreNames)
    {
        if (!await dbContext.Genres.AnyAsync(g => g.Name == genreName))
        {
            dbContext.Genres.Add(new Genre { Name = genreName });
        }
    }
    await dbContext.SaveChangesAsync();

    var authors = await dbContext.Authors.OrderBy(a => a.AuthorId).ToListAsync();
    var genres = await dbContext.Genres.OrderBy(g => g.GenreId).ToListAsync();

    var titles = new[]
    {
        "Animal Farm", "Clean Code", "Database Systems", "Programming C#", "ASP.NET Core MVC",
        "Web Design Basics", "Secure Web Apps", "Data Modelling", "Modern JavaScript", "Cloud Web Systems",
        "Software Testing", "Human Computer Interaction", "Information Systems", "Network Security", "Agile Projects",
        "Object Oriented Design", "SQL Fundamentals", "Entity Framework Guide", "Responsive Design", "Library Systems",
        "Cybersecurity Essentials", "C# Practice Guide", "MVC Project Builder", "Data Privacy Basics", "HTML and CSS Workshop",
        "JavaScript for Beginners", "Software Requirements", "Systems Analysis", "Cloud Security", "Database Administration",
        "Web Application Security", "UX Design Notes", "Programming Logic", "Advanced Forms", "Secure Login Systems",
        "Report Writing for IT", "Borrowing System Design", "Fine Management System", "Reservation Workflow", "Book Catalogue Design",
        "Feedback Systems", "Library Reports", "Digital Library Services", "Search by ISBN", "Web Hosting Basics",
        "Debugging in Visual Studio", "GitHub for Teams", "Testing MVC Apps", "Ethical Web Development", "Accessibility in Web Design",
        "Data Annotation Basics", "Controller Actions", "Razor Views", "SQL Relationships", "Practical C# Examples",
        "Modern Web Patterns", "Library Admin Guide", "Member Portal Design", "Book Cover Management", "Security Checklist"
    };

    // The browse page only shows available books. If an old database has only a few books,
    // this adds enough extra demo books so the member page has 60 available records to browse.
    var availableCount = await dbContext.Books.CountAsync(b => b.IsAvailable);
    var nextIndex = await dbContext.Books.CountAsync();
    for (int i = 0; availableCount < 60 && i < 200; i++)
    {
        var isbn = $"97810000{(nextIndex + i + 1).ToString("D5")}";
        if (await dbContext.Books.AnyAsync(b => b.ISBN == isbn))
        {
            continue;
        }

        dbContext.Books.Add(new Book
        {
            Title = titles[(nextIndex + i) % titles.Length],
            ISBN = isbn,
            IsAvailable = true,
            CoverImagePath = $"/images/books/book{((nextIndex + i) % 60) + 1}.svg",
            AuthorId = authors[(nextIndex + i) % authors.Count].AuthorId,
            GenreId = genres[(nextIndex + i) % genres.Count].GenreId
        });
        availableCount++;
    }

    var allBooksForCovers = await dbContext.Books.OrderBy(b => b.BookId).ToListAsync();
    for (int i = 0; i < allBooksForCovers.Count; i++)
    {
        if (string.IsNullOrWhiteSpace(allBooksForCovers[i].CoverImagePath) || allBooksForCovers[i].CoverImagePath!.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
        {
            allBooksForCovers[i].CoverImagePath = $"/images/books/book{(i % 60) + 1}.svg";
        }
    }
    await dbContext.SaveChangesAsync();

    if (!await dbContext.BorrowRules.AnyAsync())
    {
        dbContext.BorrowRules.Add(new BorrowRule { MaxBooks = 5, LoanDays = 14, FinePerDay = 2.00m });
        await dbContext.SaveChangesAsync();
    }

    if (!await dbContext.LibraryProfiles.AnyAsync())
    {
        dbContext.LibraryProfiles.Add(new LibraryProfile
        {
            Name = "KOI Online Library",
            Location = "Sydney",
            OperatingHours = "Mon-Fri 9:00 AM - 6:00 PM",
            ContactDetails = "library@koi.edu.au"
        });
        await dbContext.SaveChangesAsync();
    }
}

app.Run();

static async Task<bool> TableExistsAsync(ApplicationDbContext dbContext, string tableName)
{
    var connection = dbContext.Database.GetDbConnection();
    var closeConnection = connection.State != System.Data.ConnectionState.Open;

    if (closeConnection)
    {
        await connection.OpenAsync();
    }

    try
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);
        var result = await command.ExecuteScalarAsync();
        return result != null;
    }
    finally
    {
        if (closeConnection)
        {
            await connection.CloseAsync();
        }
    }
}

static async Task<bool> ColumnExistsAsync(ApplicationDbContext dbContext, string tableName, string columnName)
{
    var connection = dbContext.Database.GetDbConnection();
    var closeConnection = connection.State != System.Data.ConnectionState.Open;

    if (closeConnection)
    {
        await connection.OpenAsync();
    }

    try
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName AND COLUMN_NAME = @columnName";

        var tableParameter = command.CreateParameter();
        tableParameter.ParameterName = "@tableName";
        tableParameter.Value = tableName;
        command.Parameters.Add(tableParameter);

        var columnParameter = command.CreateParameter();
        columnParameter.ParameterName = "@columnName";
        columnParameter.Value = columnName;
        command.Parameters.Add(columnParameter);

        var result = await command.ExecuteScalarAsync();
        return result != null;
    }
    finally
    {
        if (closeConnection)
        {
            await connection.CloseAsync();
        }
    }
}


static void EnsureBookCoverFiles(string webRootPath)
{
    var booksFolder = Path.Combine(webRootPath, "images", "books");
    Directory.CreateDirectory(booksFolder);

    var defaultPath = Path.Combine(booksFolder, "default.svg");
    if (!File.Exists(defaultPath))
    {
        File.WriteAllText(defaultPath, """
<svg xmlns="http://www.w3.org/2000/svg" width="240" height="330" viewBox="0 0 240 330">
  <rect width="240" height="330" rx="18" fill="#dbeafe"/>
  <rect x="28" y="35" width="184" height="260" rx="10" fill="#ffffff" opacity="0.7"/>
  <text x="120" y="155" font-family="Arial, sans-serif" font-size="28" text-anchor="middle" fill="#1e3a8a" font-weight="700">Library</text>
  <text x="120" y="190" font-family="Arial, sans-serif" font-size="20" text-anchor="middle" fill="#1e40af">Book</text>
</svg>
""");
    }

    for (int i = 1; i <= 60; i++)
    {
        var coverPath = Path.Combine(booksFolder, $"book{i}.svg");
        if (File.Exists(coverPath))
        {
            continue;
        }

        var hue = (i * 37) % 360;
        File.WriteAllText(coverPath, $"""
<svg xmlns="http://www.w3.org/2000/svg" width="240" height="330" viewBox="0 0 240 330">
  <defs>
    <linearGradient id="g" x1="0" x2="1" y1="0" y2="1">
      <stop offset="0%" stop-color="hsl({hue}, 75%, 72%)"/>
      <stop offset="100%" stop-color="hsl({(hue + 45) % 360}, 80%, 50%)"/>
    </linearGradient>
  </defs>
  <rect width="240" height="330" rx="18" fill="url(#g)"/>
  <rect x="24" y="28" width="192" height="274" rx="12" fill="rgba(255,255,255,0.28)"/>
  <text x="120" y="140" font-family="Arial, sans-serif" font-size="26" text-anchor="middle" fill="#ffffff" font-weight="700">Library</text>
  <text x="120" y="177" font-family="Arial, sans-serif" font-size="22" text-anchor="middle" fill="#ffffff">Book {i}</text>
  <text x="120" y="240" font-family="Arial, sans-serif" font-size="16" text-anchor="middle" fill="#ffffff">ICT272</text>
</svg>
""");
    }
}
