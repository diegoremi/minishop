namespace MiniShop.ApplicationCore.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int BrandId { get; set; }

    private Product()
    {
        
    }

    public Product(string name, decimal price, int brandId)
    {
        if(string.IsNullOrEmpty(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        if (price <= 0)
            throw new ArgumentException("Product price must be greater than 0.", nameof(price));
        
        Name = name;
        Price = price;
        BrandId = brandId;
    }

    public void ChangePrice(decimal newPrice)
    {
        if(newPrice <= 0)
            throw new ArgumentException("Product price must be greater than 0.", nameof(newPrice));
        
        Price = newPrice;
    }
}