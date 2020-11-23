using System;
using System.Collections.Generic;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.GameModels.Enums;

namespace TravianTelegramBot.Calculator
{
    public static class Knowledge
    {
        public static string Rome = "Roman";
        public static string Gaul = "Gaul";
        public static string German = "German";
        public static string Trader = "Trader";

        public static Dictionary<Tribe, Dictionary<string, UnitModel>> Units = new Dictionary<Tribe, Dictionary<string, UnitModel>>
        {
            { Tribe.ROMAN, new Dictionary<string, UnitModel>
                {
                    { Trader, new UnitModel
                        {
                            Name = Trader,
                            Speed = 16
                        }
                    }
                }
            },
            { Tribe.GAUL, new Dictionary<string, UnitModel>
                {
                    { Trader, new UnitModel
                            {
                                Name = Trader,
                                Speed = 24
                            }
                    },
                    { "Falanga", new UnitModel
                        {
                            Name = "Falanga",
                            LocalizedNameEn = "Фаланга",
                            Speed = 7,
                            Attack = 15,
                            DeffenceAgainstCavalry = 50,
                            DeffenceAgainstInfantry = 40,
                            Capacity = 35,
                            Expenses = 1,
                            Tribe = Tribe.GAUL,
                            TrainingTime = TimeSpan.FromMinutes(17).Add(TimeSpan.FromSeconds(20)),
                            TrainingCost = new ResourcesModel
                            {
                                Lumber = 100,
                                Clay = 130,
                                Iron = 55,
                                Crop = 30
                            }
                        }
                    }
                }
            },
            { Tribe.TEUTON, new Dictionary<string, UnitModel>
                {
                    { Trader, new UnitModel
                            {
                                Name = Trader,
                                Speed = 12
                            }
                    }
                }
            }
        };
    }
}
