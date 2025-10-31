namespace Polly.Order.Models;

public class OrderDetails
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public DateTime SetupDate { get; set; }
    public List<Item> Items { get; set; }
}
