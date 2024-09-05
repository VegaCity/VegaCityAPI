using Microsoft.AspNetCore.Mvc;

namespace VegaCityApp.API.Controllers.OwnerStore
{
    public class OwnerStoreController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
