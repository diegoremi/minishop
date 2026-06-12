using Microsoft.AspNetCore.Mvc;
using MiniShop.ApplicationCore.Entities;
using MiniShop.ApplicationCore.Interfaces;

namespace MiniShop.WebApi.Brands;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : Controller
{
    private readonly IRepository<Brand> _brandRepository;
    
    public BrandsController(IRepository<Brand> brandRepository)
    {
        _brandRepository = brandRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<BrandDto>>> GetAll()
    {
        var brands = await _brandRepository.ListAsync();

        var response = brands
            .Select(b => new BrandDto(b.Id, b.Name))
            .ToList();
        
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BrandDto>> GetById(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);

        if (brand is null)
            return NotFound();
        
        return Ok(new BrandDto(brand.Id, brand.Name));
    }

    [HttpPost]
    public async Task<ActionResult<BrandDto>> Create(CreateBrandRequest request)
    {
        var brand = new Brand(request.Name);

        await _brandRepository.AddAsync(brand);
        
        var response = new BrandDto(brand.Id, brand.Name);
        
        return CreatedAtAction(nameof(GetById), new { id = brand.Id }, response);
    }
}