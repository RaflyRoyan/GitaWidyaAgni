using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Information
{
    /// <summary>
    /// Banner landing page (t_banner). Gambar wajib, maksimal 5 MB, harus JPEG / PNG / WebP dan disimpan apa adanya (tidak diperkecil
    /// supaya tetap tajam sebagai hero); tidak dimuat di daftar (GetImage / UpdateImage). Seq 0 = di akhir. StartAt kosong = langsung tayang,
    /// EndAt kosong = tanpa batas. Teks tombol butuh link; link berupa URL http(s) atau path diawali "/".
    /// GetActiveList untuk landing page: aktif dan sekarang di dalam jadwal, urut Seq. Hapus permanen.
    /// </summary>
    public partial class Banner : BaseLib
    {
        public class NewRow
        {
            public string Title { get; set; } = null!;
            public string? Subtitle { get; set; }
            public byte[] Image { get; set; } = null!;
            public string? ButtonText { get; set; }
            public string? LinkUrl { get; set; }
            public int Seq { get; set; }
            public DateTime? StartAt { get; set; }
            public DateTime? EndAt { get; set; }
            public ActiveStatus IsActive { get; set; } = ActiveStatus.Aktif;
        }
        public record ImageContent(byte[] Data, string ContentType);

        public const int MAX_IMAGE_BYTES = 5 * 1024 * 1024;

        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Subtitle { get; set; }
        public string? ButtonText { get; set; }
        public string? LinkUrl { get; set; }
        public int Seq { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; private set; }

        // Gambar (blob) tidak ikut di-select.
        private static IQueryable<EF.TBanner> _Querys
        {
            get
            {
                return DBClass.GetContext().TBanners.Select(x => new EF.TBanner
                {
                    Id = x.Id,
                    Title = x.Title,
                    Subtitle = x.Subtitle,
                    ButtonText = x.ButtonText,
                    LinkUrl = x.LinkUrl,
                    Seq = x.Seq,
                    StartAt = x.StartAt,
                    EndAt = x.EndAt,
                    IsActive = x.IsActive,
                    InsDate = x.InsDate
                });
            }
        }
        public Banner()
        {
        }
        public Banner(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Banner>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Banner(EF.TBanner row)
        {
            SetData(row);
        }
        private void SetData(EF.TBanner row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Banner>();
            Title = row.Title;
            Subtitle = row.Subtitle;
            ButtonText = row.ButtonText;
            LinkUrl = row.LinkUrl;
            Seq = row.Seq;
            StartAt = row.StartAt;
            EndAt = row.EndAt;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
        }
    }
}
