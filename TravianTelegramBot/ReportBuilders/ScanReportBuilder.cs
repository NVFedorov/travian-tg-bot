using System;
using System.Text;
using TTB.Common.Extensions;
using TTB.DAL.Models;
using TTB.DAL.Models.ScenarioModels;
using TTB.Gameplay.Models.ContextModels;

namespace TravianTelegramBot.ReportBuilders
{
    public class ScanReportBuilder
    {
        private readonly string _userName;
        private readonly string _timeZoneId;

        private ScanReportBuilder(string userName, string timeZoneId) {
            _userName = userName;
            _timeZoneId = timeZoneId;
        }

        public string ReportText { get; private set; }

        public static ScanReportBuilder Create(string userName, string timeZoneId)
        {
            return new ScanReportBuilder(userName, timeZoneId)
            {
                ReportText = $"Player {userName} has been scanned"
            };
        }

        public ScanReportBuilder WithNotification(Incoming notification)
        {
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine(this.ReportText);
            strBuilder.AppendLine("ALERT:");
            strBuilder.AppendLine($"Village [{notification.VillageName}] has been scanned.");
            strBuilder.AppendLine($"Date Time: [{notification.DateTime.ToDisplayStringApplyTimeZone(_timeZoneId)}]");
            strBuilder.AppendLine($"Intruder details:");
            strBuilder.AppendLine($"Player: [{notification.IntruderDetails.PlayerName}]");
            strBuilder.AppendLine($"Alliance: [{notification.IntruderDetails.Alliance}]");
            strBuilder.AppendLine($"Village: [{notification.IntruderDetails.Name}]");
            strBuilder.AppendLine($"Village URL: [{notification.IntruderVillageUrl}]");

            this.ReportText = strBuilder.ToString();
            return this;
        }
    }
}
