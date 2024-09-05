using Microsoft.AspNetCore.Mvc;

namespace VegaCityApp.API.Controllers.CashierWeb
{
    public class CashierWebController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
