using ManualMate.Application.DTOs;
using ManualMate.Application.Interfaces;
using ManualMate.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManualMate.API.Controllers
{
    [Route("api/item")]
    [ApiController]
    public class ItemController(IQaService QaService,
        ItemService itemService,
        IFileProcessingService fileProcessingService,
        FileUploadService fileUploadService) : ControllerBase
    {
        #region CRUD
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var result = await itemService.GetItemAsync(id);
            return this.GetResponse<GetItemDto>(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var result = await itemService.GetItemsAsync();
            return this.GetResponse<IEnumerable<GetItemDto>>(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem(CreateItemDto dto)
        {
            var result = await itemService.CreateItemAsync(dto);
            return this.GetResponse<CreateItemDto>(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateItem(int id, CreateItemDto dto)
        {
            var result = await itemService.EditItemAsync(id, dto);
            return this.GetResponse<GetItemDto>(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var result = await itemService.DeleteAsync(id);
            return this.GetResponse<bool>(result);
        } 
        #endregion

        [HttpPost("upload-file/{id:int}")]
        public async Task<IActionResult> UploadFile(int id, IFormFile file)
        {
            var result = await fileUploadService.UploadItemFileAsync(id, file);
            return this.GetResponse<string>(result);
        }

        [HttpGet("process-file/{id:int}")]
        public async Task<IActionResult> ProcessFile(int id)
        {
            var result = await fileProcessingService.ProcessFileAsync(id);
            return this.GetResponse<bool>(result);
        }

        [HttpDelete("embeddings/{id:int}")]
        public async Task<IActionResult> DeleteItemEmbeddings(int id)
        {
            var result = await fileProcessingService.DeleteFileEmbeddingsAsync(id);
            return this.GetResponse<bool>(result);
        }

        [HttpGet("ask/{itemId:int}")]
        public async Task<IActionResult> Ask(int itemId, string question)
        {
            var result = await QaService.GetAnswerAsync(itemId, question);
            return this.GetResponse<string>(result);
        }
    }
}