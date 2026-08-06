using ManualMate.API.Controllers;
using ManualMate.Application.DTOs;
using ManualMate.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManualMate.Controllers
{
    [Route("api/item")]
    [ApiController]
    public class ItemController(IItemService itemService) : ControllerBase
    {
        #region CRUD
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetItem(Guid id)
        {
            var result = await itemService.GetItemAsync(id);
            return this.GetResponse(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var result = await itemService.GetItemsAsync();
            return this.GetResponse(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem(CreateItemDto dto)
        {
            var result = await itemService.CreateItemAsync(dto);
            return this.GetResponse(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateItem(Guid id, UpdateItemDto dto)
        {
            var result = await itemService.EditItemAsync(id, dto);
            return this.GetResponse(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            var result = await itemService.DeleteAsync(id);
            return this.GetResponse(result);
        } 
        #endregion
    }
}