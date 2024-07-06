using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsusFanControl.Model.Enums
{
    public static class MyEnumExtensions
    {
        public static string ToDescriptionString(this Enum val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
               .GetType()
               .GetField(val.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            Type type = enumValue.GetType();
            FieldInfo fieldInfo = type.GetField(enumValue.ToString());
            DisplayAttribute[] displayAttributes = (DisplayAttribute[])fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false);

            if (displayAttributes.Length > 0)
            {
                return displayAttributes[0].Name;
            }
            else
            {
                return enumValue.ToString();
            }
        }

        public static Dictionary<int, string> EnumToDictionaryWithDescription<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                       .Cast<T>()
                       .ToDictionary(
                           e => Convert.ToInt32(e),
                           e => ToDescriptionString(e)
                       );
        }
    }
}
