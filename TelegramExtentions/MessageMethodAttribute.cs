using Telegram.Bot.Types.Enums;

namespace AccountingTer.TelegramExtentions
{
    public class MessageMethodAttribute : Attribute
    {
        public string Command { get; private set; }
        public UpdateType CommandType { get; private set; }

        public MessageMethodAttribute(string command, UpdateType updateType)
        {
            CommandType = updateType;
            Command = command;
        }
    }
}
