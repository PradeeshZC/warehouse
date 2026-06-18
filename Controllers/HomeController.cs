using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using warehouse.Models;

namespace warehouse.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                // Not authenticated -> redirect to login
                return RedirectToAction("Login", "Auth");
            }

            // Authenticated -> redirect to dashboard
            return RedirectToAction("Index", "Dashboard");
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
