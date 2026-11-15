using GWAShared.Extensions;
using GWAShared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi
{
    internal static class TransaksiHelper
    {
        internal static void ValidateStatus<TEnum>(TEnum current, string action, params TEnum[] allowed) where TEnum : struct, Enum
        {
            if (!allowed.Contains(current))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.STATUS_ACTION_INVALID, action, current.GetDescription()));
            }
        }
        // Teks bebas dipotong sesuai panjang kolom (pesan error / log job bisa lebih panjang dari kolomnya).
        internal static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            var text = value.Trim();
            return text.Length <= maxLength ? text : text[..maxLength];
        }
    }
}
