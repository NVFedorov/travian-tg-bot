using System;
using System.Text;
using TTB.Common.Extensions;
using TTB.DAL.Models;
using TTB.DAL.Models.ScenarioModels;
using TTB.DAL.Models.ScenarioModels.Enums;
using TTB.Gameplay.Models.ContextModels;

namespace TravianTelegramBot.ReportBuilders
{
    public class IncomingAttackReportBuilder
    {
        private readonly string _userName;
        private readonly string _timeZoneId;

        private IncomingAttackReportBuilder(string userName, string timeZoneId)
        {
            _userName = userName;
            _timeZoneId = timeZoneId;
        }

        public string ReportText { get; private set; }

        public static IncomingAttackReportBuilder Create(string userName, string timeZoneId)
        {
            return new IncomingAttackReportBuilder(userName, timeZoneId);
        }

        public IncomingAttackReportBuilder WithNotification(Incoming notification)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Player {_userName} is under attack:");
            sb.AppendLine(AttackNotificationToText(notification));
            ReportText = sb.ToString();
            return this;
        }

        private string AttackNotificationToText(Incoming notification)
        {
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine($"The village [{notification.VillageName}] is under attack");
            strBuilder.AppendLine($"The nearest attack date time: [{notification.DateTime.ToDisplayStringApplyTimeZone(_timeZoneId)}]");
            strBuilder.AppendLine($"The attack discovered at: [{DateTimeOffset.UtcNow.ToDisplayStringApplyTimeZone(_timeZoneId)}]");

            return strBuilder.ToString();
        }
    }
}
