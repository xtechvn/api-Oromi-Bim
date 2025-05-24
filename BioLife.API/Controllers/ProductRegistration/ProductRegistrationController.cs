using Microsoft.AspNetCore.Mvc;

namespace HuloToys_Service.Controllers.ProductRegistration
{
    public class ProductRegistrationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
