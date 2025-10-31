using Microsoft.AspNetCore.Mvc;

namespace Polly.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    private Dictionary<int, string> _customerNameDict = null;

    public CustomerController()
    {
        if (_customerNameDict == null)
        {
            _customerNameDict = new Dictionary<int, string>();
            _customerNameDict.Add(1, "Pro Code Guide");
            _customerNameDict.Add(2, "Support - Pro Code Guide");
            _customerNameDict.Add(3, "Sanjay");
            _customerNameDict.Add(4, "Sanjay - Pro Code Guide");
        }
    }

    [HttpGet]
    [Route("GetCustomerName/{customerCode}")]
    public ActionResult<string> GetCustomerName(int customerCode)
    {
        if (_customerNameDict != null && _customerNameDict.ContainsKey(customerCode))
        {
            return _customerNameDict[customerCode];
        }
        return "Customer Not Found";
    }
}
