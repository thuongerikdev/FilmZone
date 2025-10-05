using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Payment
{
    public class InvoiceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
