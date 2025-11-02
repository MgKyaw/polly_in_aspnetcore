using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using Polly.Fallback;
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
    private readonly FallbackPolicy<string> _fallbackPolicy;
    private static CircuitBreakerPolicy _circuitBreakerPolicy;

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

        _fallbackPolicy = Policy<string>
                        .Handle<Exception>()
                        .Fallback("Customer Name Not Available - Please retry later");

        _circuitBreakerPolicy = Policy.Handle<Exception>()
                                        .CircuitBreaker(2, TimeSpan.FromMinutes(1));

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

    [HttpGet]
    [Route("GetOrderByCustomerWithTimeout/{customerCode}")]
    public OrderDetails GetOrderByCustomerWithTimeout(int customerCode)
    {
        try
        {
            _httpClient = _httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(apiurl);
            var uri = "/api/Customer/GetCustomerNameWithDelay/" + customerCode;
            var result = _timeoutPolicy.Execute(() => _httpClient.GetStringAsync(uri).Result);

            _orderDetails.CustomerName = result;

            return _orderDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excpetion Occurred");
            _orderDetails.CustomerName = "Customer Name Not Available as of Now";
            return _orderDetails;
        }
    }

    [HttpGet]
    [Route("GetOrderByCustomerWithFallback/{customerCode}")]
    public OrderDetails GetOrderByCustomerWithFallback(int customerCode)
    {
        _httpClient = _httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(apiurl);
        var uri = "/api/Customer/GetCustomerNameWithPermFailure/" + customerCode;
        var result = _fallbackPolicy.Execute(() => _httpClient.GetStringAsync(uri).Result);

        _orderDetails.CustomerName = result;

        return _orderDetails;
    }

    [HttpGet]
    [Route("GetOrderByCustomerWithCircuitBreaker/{customerCode}")]
    public OrderDetails GetOrderByCustomerWithCircuitBreaker(int customerCode)
    {
        try
        {
            _httpClient = _httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(apiurl);
            var uri = "/api/Customer/GetCustomerNameWithPermFailure/" + customerCode;
            var result = _circuitBreakerPolicy.Execute(() => _httpClient.GetStringAsync(uri).Result);

            _orderDetails.CustomerName = result;
            return _orderDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception Occurred");
            _orderDetails.CustomerName = "Customer Name Not Available as of Now";
            return _orderDetails;
        }
    }
}
