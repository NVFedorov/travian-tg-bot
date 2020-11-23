using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TTB.DAL.Models.GameModels;

namespace TravianTelegramBot.ViewModels
{
    public class UnitSelectionViewModel
    {
        public Guid Id { get; set; }
        public IEnumerable<UnitModel> Units { get; set; }
        public IDictionary<string, int> Selected { get; set; }
        public bool WithQuantity { get; set; }
    }
}
