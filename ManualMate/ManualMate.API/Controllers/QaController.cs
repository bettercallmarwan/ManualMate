using ManualMate.API.Controllers;
using ManualMate.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManualMate.Controllers
{
    [Route("api/item")]
    [ApiController]
    public class QaController(IQaService qaService) : ControllerBase
    {
        [HttpGet("ask/{itemId:guid}")]
        public async Task<IActionResult> Ask(Guid itemId, string question)
        {
            var result = await qaService.GetAnswerAsync(itemId, question);
            return this.GetResponse(result);
        }
    }
}