using Microsoft.AspNetCore.Mvc;
using MiniShop.ApplicationCore.Entities;
using MiniShop.ApplicationCore.Interfaces;
using MiniShop.WebApi.Caching;

namespace MiniShop.WebApi.Products;

[ApiController]
[Route("api/products")]
public class ProductsController : Controller
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Brand> _brandRepository;
    private readonly ICacheService _cacheService;
    
    public ProductsController(
        IRepository<Product> productRepository, 
        IRepository<Brand> brandRepository,
        ICacheService cacheService)
    {
        _productRepository = productRepository;
        _brandRepository = brandRepository;
        _cacheService = cacheService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
        const string cacheKey = "products:all";

        var cachedProducts = await _cacheService.GetAsync<List<ProductDto>>(cacheKey);

        if (cachedProducts is not null)
            return Ok(cachedProducts);
        
        var products = await _productRepository.ListAsync();
        
        var response = products
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.BrandId))
            .ToList();
        
        await _cacheService.SetAsync(
            cacheKey,
            response,
            TimeSpan.FromMinutes(5)
        );
        
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var cacheKey = $"products:{id}";

        var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);

        if (cachedProduct is not null)
            return Ok(cachedProduct);
        
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
            return NotFound();

        var response = new ProductDto(
            product.Id,
            product.Name,
            product.Price,
            product.BrandId
        );
        
        await _cacheService.SetAsync(
            cacheKey,
            response,
            TimeSpan.FromMinutes(5)
        );
        
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request)
    {
        var brand = await _brandRepository.GetByIdAsync(request.BrandId);
        
        if (brand is null)
            return BadRequest($"Brand with id {request.BrandId} does not exist.");
        
        var product = new Product(
            request.Name,
            request.Price,
            request.BrandId
        );

        await _productRepository.AddAsync(product);
        await _cacheService.RemoveAsync("products:all");

        var response = new ProductDto(
            product.Id,
            product.Name,
            product.Price,
            product.BrandId
        );
        
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }
}