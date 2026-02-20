using System;

using AutoMapper;
using Core.DTOs.Product;
using Core.DTOs.Promotion;
using Core.DTOs.Schedule;
using Core.Entities;

namespace Api.Mapping;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Product
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>()
            .ForAllMembers(opt => opt.Condition(
                (src, dest, srcMember) => srcMember != null));

        // Promotion
        CreateMap<Promotion, PromotionDto>();
        CreateMap<CreatePromotionDto, Promotion>();
        CreateMap<UpdatePromotionDto, Promotion>()
            .ForAllMembers(opt => opt.Condition(
                (src, dest, srcMember) => srcMember != null));

        // ProductMappingProfile.cs
        CreateMap<Product, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PriceHistories, opt => opt.Ignore());

        CreateMap<PriceHistory, PriceHistoryDto>();
        CreateMap<Schedule, ScheduleDto>();
        CreateMap<CreateScheduleDto, Schedule>();
        CreateMap<UpdateScheduleDto, Schedule>();
        
            
    }
}