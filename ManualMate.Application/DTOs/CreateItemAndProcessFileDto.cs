using Microsoft.AspNetCore.Http;

namespace ManualMate.Application.DTOs
{
    public class CreateItemAndProcessFileDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile File { get; set; }
    }
}
