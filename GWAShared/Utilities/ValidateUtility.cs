using GWAShared.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GWAShared.Utilities
{
    public class ValidateUtility
    {
        public static void ValidateDAModel<O>(O source)
        {
            var props = typeof(O).GetProperties().Where(x => x.PropertyType == typeof(string)).ToArray();

            foreach (var prop in props)
            {
                var obj = prop.GetCustomAttribute<StringLengthAttribute>();
                if (obj is not null)
                {
                    string? objval = (string?)prop.GetValue(source);
                    if (objval is not null)
                    {
                        if (obj.MaximumLength < objval.Length)
                        {
                            throw new ArgumentException(
                                string.Format(ExceptionMessage.MODEL_STRING_LENGHT_NOTVALID,
                                prop.Name, obj.MaximumLength.ToString()));
                        }
                    }

                }
            }
        }
    }
}
