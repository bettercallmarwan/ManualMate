using ManualMate.Application.DTOs;
using ManualMate.Application.Interfaces;
using ManualMate.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManualMate.API.Controllers
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
            var result = await productService.GetProductAsync(id);
            if (!result.Success)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var result = await productService.GetProductsAsync();
            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto dto)
        {
            var result = await productService.CreateProductAsync(dto);
            return Ok(result.Value);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, CreateProductDto dto)
        {
            var result = await productService.EditProductAsync(id, dto);
            if (!result.Success)
                return NotFound(new { error = result.Error });
            return Ok(result.Value);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await productService.DeleteAsync(id);
            if (!result.Success)
                return NotFound(new { error = result.Error });
            return Ok(result.Value);
        }

        [HttpPost("upload-manual/{id:int}")]
        public async Task<IActionResult> UploadManual(int id, IFormFile file)
        {
            var result = await fileUploadService.UploadProductManualAsync(id, file);
            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpGet("process-manual/{id:int}")]
        public async Task<IActionResult> ProcessManual(int id)
        {
            var result = await manualProcessingService.ProcessManualAsync(id);

            if (!result.Success)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpGet("ask/{productId:int}")]
        public async Task<IActionResult> Ask(int productId, string question)
        {
            var result = await manualQaService.GetAnswerAsync(productId, question);
            return Ok(result.Value);
        }
    }
}