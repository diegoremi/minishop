namespace MiniShop.WebApi.Brands;

public record BrandDto(
    int Id,
    string Name
);

public record CreateBrandRequest(
    string Name
);