using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models.Entities;

namespace POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly POSDbContext _context;
        public ProductController(POSDbContext context)
        {
            _context = context;
        } 
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProductsAsync()
        {
           var products = await _context.Products.ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NotFound(new { Message = "No products found." });
            }
            return Ok(products);
        }
        [HttpPost]
        public async Task<IActionResult> AddProductAsync([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest(new { Message = "Invalid product data." });
            }
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProductsAsync), new { id = product.ProductId }, product);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductByIdAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound(new { Message = "Product not found." });
            }
            return Ok(product);
        }
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProductAsync(int productId, [FromBody] Product updatedProduct)
        {
            if (updatedProduct == null || productId != updatedProduct.ProductId)
            {
                return BadRequest(new { Message = "Invalid product data." });
            }
            var existingProduct = await _context.Products.FindAsync(productId);
            if (existingProduct == null)
            {
                return NotFound(new { Message = "Product not found." });
            }
            existingProduct.ProductName = updatedProduct.ProductName;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.Quantity = updatedProduct.Quantity;
            existingProduct.isAvailable = updatedProduct.isAvailable;
            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();
            return StatusCode (200, "Product updated");
        }
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound(new { Message = "Product not found." });
            }
            product.isAvailable = false;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
