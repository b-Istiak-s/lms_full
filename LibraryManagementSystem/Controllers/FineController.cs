using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    // Person 4: Member fine calculator page.
    public class FineController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(FineCalculator model)
        {
            const double finePerDay = 2.00;
            model.Amount = model.DaysLate * finePerDay;
            ViewBag.Result = $"Estimated fine for {model.MemberName} is ${model.Amount:0.00}.";
            return View(model);
        }
    }
}
