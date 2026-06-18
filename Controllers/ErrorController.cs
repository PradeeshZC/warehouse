#nullable enable
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.ViewModels;

namespace Warehouse.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet("/Home/Error")]
        public IActionResult Index()
        {
            var vm = new ErrorViewModel { RequestId = HttpContext.TraceIdentifier };
            return View("Error", vm);
        }
    }
}
