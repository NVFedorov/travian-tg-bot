using System;
using System.Collections.Generic;
using System.Linq;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.PlayerModels;
using TTB.Gameplay.Models.ContextModels;

namespace TravianTelegramBot.Calculator
{
    public static class Calculator
    {
        public static TimeSpan CalculateTimeForUnit(int startX, int startY, int finishX, int finishY, UnitModel unit)
        {
            var distance = Math.Sqrt(Math.Pow(startX - finishX, 2) + Math.Pow(startY - finishY, 2));
            var time = distance / unit.Speed;
            return TimeSpan.FromHours(time);
        }

        public static TimeSpan CalculateTimeForUnit(Village village1, Village village2, UnitModel unit)
        {
            return CalculateTimeForUnit(village1.CoordinateX, village1.CoordinateY, village2.CoordinateX, village2.CoordinateY, unit);
        }

        public static VillageModel GetNearestVillage(IEnumerable<VillageModel> all, VillageModel start)
        {
            var others = all.Where(x => x.CoordinateX != start.CoordinateX && x.CoordinateY != start.CoordinateY);
            var min = double.MaxValue;
            VillageModel result = null;

            foreach(var current in others)
            {
                var d = Distance(start, current);
                if (d < min)
                {
                    result = current;
                    min = d;
                }
            }

            return result;
        }

        private static double Distance(VillageModel v1, VillageModel v2)
        {
            return Math.Sqrt(Math.Pow(v1.CoordinateX - v2.CoordinateX, 2) + Math.Pow(v1.CoordinateY - v2.CoordinateY, 2));
        }
    }
}
