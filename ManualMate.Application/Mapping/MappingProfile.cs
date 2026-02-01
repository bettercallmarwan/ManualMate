using AutoMapper;
using ManualMate.Application.DTOs;
using ManualMate.Domain.Models;

namespace ManualMate.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //CreateMap<FileEmbedding, FileEmbeddingDto>();
            CreateMap<GetItemDto, Item>().ReverseMap();
            CreateMap<CreateItemDto, Item>().ReverseMap();
            CreateMap<CreateItemAndProcessFileDto, Item>();
            CreateMap<CreateItemAndProcessFileDto, CreateItemDto>();
            CreateMap<Item, ItemResponseDto>().ReverseMap();
            CreateMap<CreateItemAndProcessFileDto, ItemResponseDto>().ReverseMap();



        }
    }
}