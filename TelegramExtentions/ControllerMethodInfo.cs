using System.Reflection;

namespace AccountingTer.TelegramExtentions
{
    internal class ControllerMethodInfo
    {
        public MessageMethodAttribute Attribute { get; }
        public MethodInfo Method { get; }

        public ControllerMethodInfo(MessageMethodAttribute attribute, MethodInfo method)
        {
            Attribute = attribute;
            Method = method;
        }

    }
}
