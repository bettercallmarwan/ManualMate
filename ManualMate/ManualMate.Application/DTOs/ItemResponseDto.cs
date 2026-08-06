namespace ManualMate.Application.DTOs
{
    public record ItemResponseDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
    }
}
