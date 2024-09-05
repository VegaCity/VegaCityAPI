using Microsoft.AspNetCore.Mvc;

namespace VegaCityApp.API.Controllers.CashierApp
{
    public class CashierAppController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
