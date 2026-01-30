namespace ManualMate.Application.DTOs
{
    public class CreateItemDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string? FilePath { get; set; }
    }
}