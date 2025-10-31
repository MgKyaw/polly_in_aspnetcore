using Microsoft.AspNetCore.Mvc;

namespace Polly.Customer.Controllers;
public class CustomerController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
