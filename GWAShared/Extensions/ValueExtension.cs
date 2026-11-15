using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GWAShared.Extensions
{
    public static class ValueExtension
    {
        public static void SetValue(object? objVal, object target, PropertyInfo item)
        {
            if (item is null)
            {
                return;
            }


            //var itemTarget = target.GetType().GetProperty(item.Name, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            var itemTarget = target.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.Name.ToLower() == item.Name.ToLower()
                && (x.PropertyType.FullName.StartsWith("System") || x.PropertyType.IsEnum)
                ).FirstOrDefault();

            if (itemTarget == null)
            {
                //itemTarget = target.GetType().GetProperty(item.Name);
                itemTarget = target.GetType().GetProperties()
                .Where(x => x.Name.ToLower() == item.Name.ToLower()
                && (x.PropertyType.FullName.StartsWith("System") || x.PropertyType.IsEnum)
                ).FirstOrDefault();
            }
            if (itemTarget is not null && objVal is null && itemTarget.CanWrite)
            {
                if (itemTarget.PropertyType == typeof(string))
                {
                    itemTarget.SetValue(target, null);
                }
                return;
            }
            if (itemTarget is not null && objVal is not null && itemTarget.CanWrite)
            {

                if (objVal!.GetType().IsEnum && itemTarget.PropertyType == typeof(int?))
                {
                    itemTarget.SetValue(target, (int)objVal);
                }
                else
                {
                    if (itemTarget.PropertyType.IsEnum && objVal.GetType() == typeof(bool))
                    {
                        itemTarget.SetValue(target, (((bool)objVal) ? 1 : 0));
                    }
                    else
                    {
                        itemTarget.SetValue(target, objVal);
                    }

                }
            }

        }
        public static void CopyValue<T>(this object from, T? toObj)
        {

            if (toObj is null || from is null)
            {
                return;
            }
            Type elementType = from.GetType();

            PropertyInfo[] properties = elementType.GetProperties().Where(x => x.PropertyType is not null && (x.PropertyType.FullName.StartsWith("System") || x.PropertyType.IsEnum)).ToArray();

            foreach (var item in properties)
            {
                var objVal = item.GetValue(from);
                SetValue(objVal, toObj, item);
            }
        }
    }
}
