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
        // Ubah status (nilai enum) untuk baris berkunci encKey.
        public class StatusInput
        {
            public string Key { get; set; } = "";
            public int Status { get; set; }
            public string? Keterangan { get; set; }
        }
        /// <summary>
        /// Form pembayaran / pencairan bersama (Views/Shared/_FormBayar.cshtml): akun kas, kategori, tanggal, nominal, catatan.
        /// Halaman memakai wrapper _Bayar.cshtml dan memanggil OnSave (fungsi JS) yang mengirim formData #frmBayar.
        /// </summary>
        public class BayarForm
        {
            public string Judul { get; set; } = "";
            public string? Info { get; set; }
            public string OnSave { get; set; } = "";
            public string Tombol { get; set; } = "Simpan";
            public bool ShowAmount { get; set; } = true;
            public bool ShowCategory { get; set; } = true;
            public bool CategoryRequired { get; set; } = true;
            public bool AccountRequired { get; set; } = true;
            public bool ShowNotes { get; set; }
            public List<OpsiItem> AkunOpsi { get; set; } = GitaWidyaAgniWeb.Models.Opsi.Akun();
            public List<OpsiItem> KategoriOpsi { get; set; } = new();
            public BayarInput Row { get; set; } = new();
        }
        public class BayarInput
        {
            public string Key { get; set; } = "";

            [System.ComponentModel.DataAnnotations.Display(Name = "Akun Kas")]
            public int? AccountId { get; set; }

            [System.ComponentModel.DataAnnotations.Display(Name = "Kategori")]
            public int? CategoryId { get; set; }

            [System.ComponentModel.DataAnnotations.Display(Name = "Tanggal")]
            public DateTime? Date { get; set; } = DateTime.Today;

            [System.ComponentModel.DataAnnotations.Display(Name = "Nominal (Rp)")]
            public decimal? Amount { get; set; }

            [System.ComponentModel.DataAnnotations.Display(Name = "Catatan")]
            public string? Notes { get; set; }
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
