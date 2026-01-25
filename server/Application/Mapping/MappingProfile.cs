using AutoMapper;
using ManualMate.Application.DTOs;
using ManualMate.Domain.Models;

namespace ManualMate.Application.Mapping
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