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
        #region CRUD
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var result = await productService.GetProductAsync(id);
            return this.GetResponse<GetProductDto>(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var result = await productService.GetProductsAsync();
            return this.GetResponse<IEnumerable<GetProductDto>>(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto dto)
        {
            var result = await productService.CreateProductAsync(dto);
            return this.GetResponse<CreateProductDto>(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, CreateProductDto dto)
        {
            var result = await productService.EditProductAsync(id, dto);
            return this.GetResponse<GetProductDto>(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await productService.DeleteAsync(id);
            return this.GetResponse<bool>(result);
        } 
        #endregion

        [HttpPost("upload-manual/{id:int}")]
        public async Task<IActionResult> UploadManual(int id, IFormFile file)
        {
            var result = await fileUploadService.UploadProductManualAsync(id, file);
            return this.GetResponse<string>(result);
        }

        [HttpGet("process-manual/{id:int}")]
        public async Task<IActionResult> ProcessManual(int id)
        {
            var result = await manualProcessingService.ProcessManualAsync(id);
            return this.GetResponse<bool>(result);
        }

        [HttpDelete("embeddings/{id:int}")]
        public async Task<IActionResult> DeleteProductEmbeddings(int id)
        {
            var result = await manualProcessingService.DeleteProductEmbeddingsAsync(id);
            return this.GetResponse<bool>(result);
        }

        [HttpGet("ask/{productId:int}")]
        public async Task<IActionResult> Ask(int productId, string question)
        {
            var result = await manualQaService.GetAnswerAsync(productId, question);
            return this.GetResponse<string>(result);
        }
    }
}