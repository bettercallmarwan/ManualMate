using AutoMapper;
using ManualMate.DTOs;
using ManualMate.Models;

namespace ManualMate.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ManualEmbedding, ManualEmbeddingDto>();
            CreateMap<GetProductDto, Product>().ReverseMap();
            CreateMap<CreateProductDto, Product>().ReverseMap();
        }
    }
}
