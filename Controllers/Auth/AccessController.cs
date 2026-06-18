#nullable enable
using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Controllers.Auth
{
    public class AccessController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult UnauthorizedPage()
        {
            return View();
        }
    }
}
