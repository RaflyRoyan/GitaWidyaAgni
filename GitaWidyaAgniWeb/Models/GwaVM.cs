using GitaWidyaAgniWeb.Infrastructure;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models
{
    /// <summary>Input dan pilihan umum halaman pengelolaan (dikirim JSON dari wwwroot/Scripts/Shared/Gwa.js).</summary>
    public class GwaVM
    {
        public class KeyInput
        {
            public string Key { get; set; } = "";
        }
        public class AlasanInput
        {
            public string Key { get; set; } = "";
            public string Keterangan { get; set; } = "";
        }
        // Pilihan SelectBox / TagBox.
        public class OpsiItem
        {
            public int Value { get; set; }
            public string Text { get; set; } = "";
            public OpsiItem() { }
            public OpsiItem(int value, string text) { Value = value; Text = text; }
        }

        public static List<OpsiItem> Opsi<T>(params T[] kecuali) where T : struct, Enum
        {
            return EnumHelper.Options(kecuali).Select(x => new OpsiItem((int)x.Value, x.Text)).ToList();
        }
        public static List<OpsiItem> OpsiAktif => Opsi<ActiveStatus>();
    }
}
