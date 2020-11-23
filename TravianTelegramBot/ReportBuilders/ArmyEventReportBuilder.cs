using System.Text;
using TTB.Common.Extensions;
using TTB.DAL.Models;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.ScenarioModels;

namespace TravianTelegramBot.ReportBuilders
{
    public class ArmyEventReportBuilder
    {
        private readonly string timezone;

        private ArmyEventReportBuilder(string timezone) {
            this.timezone = timezone;
        }

        public string ReportText { get; private set; }

        public static ArmyEventReportBuilder ForVillage(string villageName, string timezone)
        {
            var result = new ArmyEventReportBuilder(timezone)
            {
                ReportText = $"Village [{villageName}] is under attack."
            };

            return result;
        }
        public ArmyEventReportBuilder WithNotification(AttackModel attack)
        {
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine(ReportText);
            //strBuilder.AppendLine($"Village [{armyEvent.UserVillage}] is under attack.");
            strBuilder.AppendLine($"The attack date time: [{attack.DateTime.ToDisplayStringApplyTimeZone(timezone)}]");
            strBuilder.AppendLine($"The attack intruder details: [{attack.IntruderVillageUrl}]");
            if (attack.FromVillage != null)
            {
                strBuilder.AppendLine($"The intruder: [{attack.FromVillage.PlayerName}]");
                strBuilder.AppendLine($"The intruder village: [{attack.FromVillage.VillageName} [{attack.FromVillage.CoordinateX}|{attack.FromVillage.CoordinateY}]");
                strBuilder.AppendLine($"The intruder Alliance: [{attack.FromVillage.Alliance}]");
            }

            ReportText = strBuilder.ToString();
            return this;
        }
    }
}
