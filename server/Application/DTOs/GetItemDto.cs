using System.ComponentModel.DataAnnotations;

namespace ManualMate.Application.DTOs
{
    public class GetItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? FilePath { get; set; }
    }
}
