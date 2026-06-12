namespace MiniShop.WebApi.Products;

public record ProductDto(
    int Id,
    string Name,
    decimal Price,
    int BrandId
);

public record CreateProductRequest(
    string Name,
    decimal Price,
    int BrandId
);