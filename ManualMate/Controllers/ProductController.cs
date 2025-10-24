using ManualMate.Controllers.Responses;
using ManualMate.DTOs;
using ManualMate.Interfaces;
using ManualMate.Models;
using ManualMate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ManualMate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IManualQaService manualQaService,
        ProductService productService,
        IManualProcessingService manualProcessingService,
        FileUploadService fileUploadService) : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await productService.GetProductAsync(id);
            if (product == null)
                return NotFound(Result<Product>.Fail($"Product With Id : {id} Is Not Found"));

            return Ok(Result<Product>.Ok(product));
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await productService.GetProductsAsync();

            return Ok(Result<IEnumerable<Product>>.Ok(products));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductDto dto)
        {
            var productDto = await productService.CreateProductAsync(dto);
            return Ok(Result<ProductDto>.Ok(productDto));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductDto dto)
        {
            var result = await productService.EditProductAsync(id, dto);
            if (result is null) return NotFound(Result<ProductDto>.Fail($"Product With Id : {id} Is Not Found"));

            return Ok(Result<ProductDto>.Ok(dto));
        }

        [HttpPost("{id:int}/upload-manual")]
        public async Task<IActionResult> UploadManual(int id, IFormFile file)
        {
            var manualPath = await fileUploadService.UploadProductManualAsync(id, file);
            if (manualPath == null)
                return BadRequest(Result<string>.Fail("Invalid file or product not found"));

            return Ok(Result<string>.Ok(manualPath));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await productService.DeleteAsync(id);
            if (result is null) return NotFound(Result<ProductDto>.Fail($"Product With Id : {id} Is Not Found"));

            return NoContent();
        }

        [HttpGet("process-manual/{id:int}")]
        public async Task<IActionResult> ProcessManual(int id)
        {
            await manualProcessingService.ProcessManualAsync(id);
            return Ok();
        }

        [HttpGet("ask/{productId:int}")]
        public async Task<IActionResult> Ask(int productId, string question)
        {
            var result = await manualQaService.GetAnswerAsync(productId, question);
            return Ok(result);
        }
    }
}