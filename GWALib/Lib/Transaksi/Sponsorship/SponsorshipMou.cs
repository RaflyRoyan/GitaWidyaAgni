using GWALib.General;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Sponsorship
{
    /// <summary>
    /// MoU / PKS sponsorship (t_sponsorship_mou). Nomor unik; kosong = dibuat dari format MOU, atau MOU/yyyyMMdd/id bila format belum diatur.
    /// Tidak bisa ditambah / diubah bila sponsorship Batal. Dokumen (maksimal 10 MB) tidak ikut dimuat di daftar (GetDocument / UpdateDocument).
    /// Tidak bisa dihapus bila dipakai item berita acara serah terima (MouAktif). GetActiveList = MoU sponsorship Aktif yang belum berakhir.
    /// </summary>
    public partial class SponsorshipMou : BaseLib
    {
        public class NewRow
        {
            public int SponsorshipId { get; set; }
            // Kosong = nomor otomatis.
            public string? MouNumber { get; set; }
            public DateOnly? SignedDate { get; set; }
            public DateOnly? StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
            public string? Notes { get; set; }
            // Opsional: file MoU.
            public byte[]? Document { get; set; }
            public string? DocumentFileName { get; set; }
            public string? DocumentContentType { get; set; }
        }
        public record FileContent(byte[] Data, string FileName, string? ContentType);

        public int Id { get; set; }
        public int SponsorshipId { get; private set; }
        public string MouNumber { get; set; } = null!;
        public DateOnly? SignedDate { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        // Terisi = dokumen ada.
        public string? DocumentFileName { get; private set; }
        public string? Notes { get; set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        // Kolom dokumen (blob) tidak ikut di-select.
        private static IQueryable<EF.TSponsorshipMou> _Querys
        {
            get
            {
                return DBClass.GetContext().TSponsorshipMous.Select(x => new EF.TSponsorshipMou
                {
                    Id = x.Id,
                    SponsorshipId = x.SponsorshipId,
                    Sponsorship = x.Sponsorship,
                    MouNumber = x.MouNumber,
                    SignedDate = x.SignedDate,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    DocumentFileName = x.DocumentFileName,
                    Notes = x.Notes,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                });
            }
        }
        public SponsorshipMou()
        {
        }
        public SponsorshipMou(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<SponsorshipMou>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal SponsorshipMou(EF.TSponsorshipMou row)
        {
            SetData(row);
        }
        private void SetData(EF.TSponsorshipMou row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<SponsorshipMou>();
            SponsorshipId = row.SponsorshipId;
            MouNumber = row.MouNumber;
            SignedDate = row.SignedDate;
            StartDate = row.StartDate;
            EndDate = row.EndDate;
            DocumentFileName = row.DocumentFileName;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
