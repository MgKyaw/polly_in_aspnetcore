using Microsoft.AspNetCore.Mvc;
using Polly.Order.Models;
using Polly.Retry;
using Polly.Timeout;

namespace Polly.Order.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RetryPolicy _retryPolicy;
    private static TimeoutPolicy _timeoutPolicy;
    private HttpClient _httpClient;
    private string apiurl = @"http://localhost:5043/";

    private OrderDetails _orderDetails = null;
    public OrderController(ILogger<OrderController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;

        _retryPolicy = Policy
            .Handle<Exception>()
            .Retry(2);

        _timeoutPolicy = Policy.Timeout(20, TimeoutStrategy.Pessimistic);

        if (_orderDetails == null)
        {
            _orderDetails = new OrderDetails
            {
                Id = 7261,
                SetupDate = DateTime.Now.AddDays(-10),
                Items = new List<Item>()
            };
            _orderDetails.Items.Add(new Item
            {
                Id = 6514,
                Name = ".NET Core Book"
            });
        }
    }

    [HttpGet]
    [Route("GetOrderByCustomer/{customerCode}")]
    public OrderDetails GetOrderByCustomer(int customerCode)
    {
        _httpClient = _httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(apiurl);
        var uri = "/api/Customer/GetCustomerName/" + customerCode;
        var result = _httpClient.GetStringAsync(uri).Result;

        _orderDetails.CustomerName = result;

        return _orderDetails;
    }

    [HttpGet]
    [Route("GetOrderByCustomerWithRetry/{customerCode}")]
    public OrderDetails GetOrderByCustomerWithRetry(int customerCode)
    {
        _httpClient = _httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(apiurl);
        var uri = "/api/Customer/GetCustomerNameWithTempFailure/" + customerCode;
        var result = _retryPolicy.Execute(() => _httpClient.GetStringAsync(uri).Result);

        _orderDetails.CustomerName = result;

        return _orderDetails;
    }
}
