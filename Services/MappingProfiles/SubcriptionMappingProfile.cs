using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class SubscriptionMappingProfile : Profile
    {
        public SubscriptionMappingProfile()
        {
            // Tier mapping
            CreateMap<SubscriptionTier, SubscriptionTierDTO>().ReverseMap();
            CreateMap<CreateSubscriptionTierDTO, SubscriptionTier>();

            // User subscription mapping
            CreateMap<UserSubscription, UserSubscriptionDTO>()
                .ForMember(dest => dest.TierName, opt => opt.MapFrom(src => src.SubscriptionTier.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.SubscriptionTier.Price))
                .ForMember(dest => dest.DurationInDays, opt => opt.MapFrom(src => src.SubscriptionTier.DurationInDays));
        }
    }
}
