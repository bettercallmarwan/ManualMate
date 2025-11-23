using System.ComponentModel.DataAnnotations;

namespace ManualMate.Application.DTOs
{
    public class CreateProductDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string? ManualPath { get; set; }
    }
}
