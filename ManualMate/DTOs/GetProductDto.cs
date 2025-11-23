using System.ComponentModel.DataAnnotations;

namespace ManualMate.DTOs
{
    public class GetProductDto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string? ManualPath { get; set; }
    }
}
