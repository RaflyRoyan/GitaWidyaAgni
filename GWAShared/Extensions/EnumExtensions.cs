using GWAShared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GWAShared.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo? field = value.GetType().GetField(value.ToString());
            if (field != null)
            {
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null)
                {
                    return attribute.Description;
                }
            }
            return value.ToString();
        }
        public static List<SelectListItemCustom> ToEnumList<TEnum>() where TEnum : struct, Enum
        {
            var type = typeof(TEnum);
            var items = new List<SelectListItemCustom>();

            foreach (var value in Enum.GetValues(type))
            {
                var field = type.GetField(value.ToString() ?? "");
                var description = field?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();

                items.Add(new SelectListItemCustom
                {
                    Value = Convert.ToInt32(value),
                    Text = value.ToString() ?? "",
                    Description = description ?? "",
                });
            }

            return items;
        }
        public static EnumItem ConvertEnumToEnumClass<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            EnumItem enumClass = new EnumItem();

            // Mengambil EnumId (nilai integer dari enum)
            enumClass.EnumId = Convert.ToInt32(enumValue);

            // Mengambil EnumName (nama string dari enum)
            enumClass.EnumName = enumValue.ToString();

            // Mengambil EnumDescription dari atribut kustom (jika ada)
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field != null)
            {
                enumClass.EnumDescription = field?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? field?.ToString() ?? "";
            }

            return enumClass;
        }
    }
}
