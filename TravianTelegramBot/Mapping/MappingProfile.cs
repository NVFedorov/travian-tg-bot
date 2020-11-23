using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OpenQA.Selenium;
using TravianTelegramBot.ViewModels;
using TTB.Common.Extensions;
using TTB.DAL.Models;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Models.ScenarioModels;
using TTB.Gameplay.Models.ContextModels;

namespace TravianTelegramBot.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Village, VillageModel>()
                .ForMember(dest => dest.VillageName, opt => opt.MapFrom(source => source.Name))
                .ForMember(dest => dest.VillageId, opt => opt.MapFrom(source => $"{source.CoordinateX}&{source.CoordinateY}"))
                .ForMember(dest => dest.NextBuildingPlanExecutionTime, opt => opt.MapFrom(source => DateTimeOffset.Now + (source.Dorf1BuildTimeLeft < source.Dorf2BuildTimeLeft ? source.Dorf1BuildTimeLeft : source.Dorf2BuildTimeLeft)))
                .ReverseMap();
            CreateMap<Resources, ResourcesModel>().ReverseMap();
            CreateMap<Incoming, AttackModel>()
                .ForMember(dest => dest.FromVillage, opt => opt.MapFrom(source => source.IntruderDetails))
                .ReverseMap();
            CreateMap<Cookie, CookieModel>()
                .ForMember(dest => dest.Expiry, opt => opt.MapFrom(source => source.Expiry.ToFormattedString()));
            CreateMap<CookieModel, Cookie>()
                .ForMember(dest => dest.Expiry, opt => opt.MapFrom(source => source.Expiry.ToDateTime()));
            CreateMap<VillageModel, VillageViewModel>()
                .ForMember(dest => dest.IsOffence, opt => opt.MapFrom(source => source.Types.Contains(VillageType.OFFENCE)))
                .ForMember(dest => dest.IsDeffence, opt => opt.MapFrom(source => source.Types.Contains(VillageType.DEFFENCE)))
                .ForMember(dest => dest.IsResource, opt => opt.MapFrom(source => source.Types.Contains(VillageType.RESOURCES)))
                .ForMember(dest => dest.IsScan, opt => opt.MapFrom(source => source.Types.Contains(VillageType.SCAN)))
                .ForMember(dest => dest.IsUnderAttack, opt => opt.MapFrom(source => source.Attacks.Any()));
            CreateMap<TravianUser, Player>()
                 .ForMember(dest => dest.Uri, opt => opt.MapFrom(source => new Uri(source.Url)))
                 .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(source => source.PlayerData == null ? string.Empty : source.PlayerData.TimeZone))
                 .ForMember(dest => dest.Tribe, opt => opt.MapFrom(source => source.PlayerData == null ? Tribe.UNDEFINED : (Tribe)source.PlayerData.Tribe));

            CreateMap<BuildingPlanModel, BuildingPlanViewModel>()
                .ForMember(x => x.BuildingSteps, opt => opt.Ignore());
            CreateMap<BuildingPlanViewModel, BuildingPlanModel>()
                .ForMember(x => x.BuildingSteps, opt => opt.Ignore());
        }
    }
}
