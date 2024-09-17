namespace AccountingTer.Models
{
    public class TelegramOptions
    {
        public const string DefaultSectionName = "TelegramOptions";

        public string Token { get; set; } 
        public string Url { get; set; }
    }
}
