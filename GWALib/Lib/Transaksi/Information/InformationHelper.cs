using GWAShared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Information
{
    internal static class InformationHelper
    {
        // Link boleh URL absolut http(s) atau path relatif yang diawali "/" (bukan "//"). Kosong = null.
        internal static string? ValidateLink(string? value, string label)
        {
            var text = TransaksiHelper.ToNullable(value);
            if (text == null)
            {
                return null;
            }
            var relative = text.StartsWith('/') && !text.StartsWith("//");
            var absolute = Uri.TryCreate(text, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            if (!relative && !absolute)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.URL_INVALID, label));
            }
            return text;
        }
    }
}
