using System.Text.RegularExpressions;

namespace AccountingTer.TelegramExtentions
{
    public static class GroupCollectionValueGetter
    {
        public static bool TryGetValue(this GroupCollection group, int index, out string outValue)
        {
            bool response = true;
            outValue = default;

            try
            {
                var data = group[index];
                if (data == null) return false;
                if(string.IsNullOrWhiteSpace(data.Value)) return false;
                outValue = data.Value;
            }
            catch
            {
                response = false;
            }
            return response;

        }
    }

}
