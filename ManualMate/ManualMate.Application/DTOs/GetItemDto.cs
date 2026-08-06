using System.ComponentModel.DataAnnotations;

namespace ManualMate.Application.DTOs
{
    public record GetItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? FilePath { get; set; }
    }
}
