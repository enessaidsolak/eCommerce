public class OrderSuccessViewModel
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Address { get; set; }
    public List<OrderProductDto> Products { get; set; }
}

public class OrderProductDto
{
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
