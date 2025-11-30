using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Shared.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            MemberInfo member = enumValue.GetType().GetMember(enumValue.ToString()).First();
            DisplayAttribute? attr = member.GetCustomAttribute<DisplayAttribute>();
            if (attr != null)
                return attr.Name!;
            return enumValue.ToString();
        }
    }
}
