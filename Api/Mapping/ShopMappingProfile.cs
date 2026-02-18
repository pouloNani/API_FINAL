using AutoMapper;
using Core.DTOs.Address;
using Core.DTOs.Shop;
using Core.Entities;

namespace Api.Mapping;

public class ShopMappingProfile : Profile
{
    public ShopMappingProfile()
    {
        // Address
        CreateMap<AddressDto, Address>();
        CreateMap<Address, AddressDto>();
        CreateMap<Shop, ShopDto>();
        // CreateShopDto → Shop
        CreateMap<CreateShopDto, Shop>()
            .ForMember(dest => dest.Address,
                       opt => opt.MapFrom(src => src.Address));

        CreateMap<CreateShopForOwnerDto, Shop>()
            .ForMember(dest => dest.Address,
                       opt => opt.MapFrom(src => src.Address));
    }
}
