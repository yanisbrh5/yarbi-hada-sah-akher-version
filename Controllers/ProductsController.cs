using API.Data;
using API.Modeles;
using API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly StoreContext _context;
        private readonly API.Services.IPhotoService _photoService;

        public ProductsController(StoreContext context, API.Services.IPhotoService photoService)
        {
            _context = context;
            _photoService = photoService;
        }

        // GET: api/Products?categoryId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] int? categoryId)
        {
            if (categoryId.HasValue)
            {
                return await _context.Products.Include(p => p.Category).Where(p => p.CategoryId == categoryId.Value).ToListAsync();
            }
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromForm] API.DTOs.CreateProductDto productDto)
        {
            string imageUrl = "";

            if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
            {
                var result = await _photoService.AddPhotoAsync(productDto.ImageFile);
                if (result.Error != null) return BadRequest(result.Error.Message);
                imageUrl = result.SecureUrl.AbsoluteUri;
            }

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                CategoryId = productDto.CategoryId,
                AvailableColors = productDto.AvailableColors ?? "",
                ImageUrl = imageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, [FromForm] API.DTOs.CreateProductDto productDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.CategoryId = productDto.CategoryId;
            product.AvailableColors = productDto.AvailableColors ?? "";

            if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
            {
                var result = await _photoService.AddPhotoAsync(productDto.ImageFile);
                if (result.Error != null) return BadRequest(result.Error.Message);
                product.ImageUrl = result.SecureUrl.AbsoluteUri;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
