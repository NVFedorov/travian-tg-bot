namespace TTB.Common.Settings
{
    public class LoggingEvents
    {
        // Information
        public const int GenerateItems = 1000;
        public const int ListItems = 1001;
        public const int GetItem = 1002;
        public const int InsertItem = 1003;
        public const int UpdateItem = 1004;
        public const int DeleteItem = 1005;
        // User audit events
        public const int UserSignedIn = 1100;
        public const int UserSignedOut = 1101;
        public const int UserIsLockedOut = 1102;
        public const int NewUserRegistred = 1110;
        public const int RegisterAttemptFailed = 1111;
        public const int RegisterAttemptFailedWithWrongSecurityCode = 1112;

        // Debug

        // Error
        public const int CriticalError = 4321;
        public const int DbOpertationException = 4000;
        public const int GetItemNotFound = 4001;
        public const int GetItemException = 4002;
        public const int UpdateItemNotFound = 4003;
        public const int UpdateItemException = 4004;
        public const int DeleteItemNotFound = 4005;
        public const int DeleteItemException = 4006;
        public const int InsertItemException = 4007;
        public const int IdentityOperationException = 4008;

        public const int CommandException = 4010;
        public const int CreateCommandException = 4011;

        // WEB API service Errors
        public const int WebApiException = 4020;
        public const int WebApiRequestFailed = 4021;

        // Telegram Client Errors
        public const int TelegramClientException = 4030;

        // BackgroundJob errors
        public const int BackgroundJobCreationException = 4100;
        public const int BackgroundJobExecutingException = 4101;
        public const int BackgroundJobInterruptionException = 4101;

        // GamePlay errors
        //public const int UpdateDataException = 10000;
        public const int ScenarioException = 10001;

    }
}
