using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    // Person 4: Member feedback and rating page.
    public class FeedbackController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(BookFeedback model)
        {
            if (model.Rating < 1 || model.Rating > 5)
            {
                ViewBag.Error = "Rating must be between 1 and 5.";
                return View(model);
            }

            ViewBag.Message = "Thank you for submitting your book feedback.";
            return View();
        }
    }
}
