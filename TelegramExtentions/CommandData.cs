using System.Text.RegularExpressions;

namespace AccountingTer.TelegramExtentions
{
    public class CommandData
    {
        public int? Value { get; set; } = null;
        public string? Description { get; set; } = null;
        public string? UserName { get; set; } = null;
        public string? Command { get; set; } = null;
        public string Text { get; set; }
        public CommandData(GroupCollection groupCollection)
        {
            if (groupCollection.TryGetValue(1, out var text))
                Text = text;
            if (groupCollection.TryGetValue(1, out var command))
                Command = command;
            if (groupCollection.TryGetValue(2, out var userName))
                UserName = userName.Replace("@", "");
            if (groupCollection.TryGetValue(3, out var strValue))
                if (int.TryParse(strValue, out var value))
                    Value = value;
            if (groupCollection.TryGetValue(4, out var description))
                Description = description;
        }
    }
}
