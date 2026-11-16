using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Sponsorship
{
    /// <summary>
    /// Checklist benefit per sponsorship (t_sponsorship_benefit). Disalin dari benefit paket saat sponsorship dibuat, boleh ditambah manual.
    /// SeqNo 0 = urutan berikutnya. Semua perubahan hanya saat sponsorship Aktif. MarkDone mencatat waktu / user dan bukti opsional (maksimal 5 MB),
    /// MarkUndone mengosongkannya. Bukti tidak ikut dimuat di daftar (GetProof / UpdateProof).
    /// </summary>
    public partial class SponsorshipBenefit : BaseLib
    {
        public class NewRow
        {
            public int SponsorshipId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            // 0 = urutan berikutnya.
            public int SeqNo { get; set; }
            public string? Notes { get; set; }
        }
        public record FileContent(byte[] Data, string FileName, string? ContentType);

        public int Id { get; set; }
        public int SponsorshipId { get; private set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int SeqNo { get; set; }
        // Hanya-baca: diubah lewat MarkDone / MarkUndone.
        public SponsorshipBenefitStatus IsDone { get; private set; }
        public DateTime? DoneAt { get; private set; }
        public int? DoneBy { get; private set; }
        // Terisi = bukti ada.
        public string? ProofFileName { get; private set; }
        public string? Notes { get; set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        // Kolom bukti (blob) tidak ikut di-select.
        private static IQueryable<EF.TSponsorshipBenefit> _Querys
        {
            get
            {
                return DBClass.GetContext().TSponsorshipBenefits.Select(x => new EF.TSponsorshipBenefit
                {
                    Id = x.Id,
                    SponsorshipId = x.SponsorshipId,
                    Title = x.Title,
                    Description = x.Description,
                    SeqNo = x.SeqNo,
                    IsDone = x.IsDone,
                    DoneAt = x.DoneAt,
                    DoneBy = x.DoneBy,
                    ProofFileName = x.ProofFileName,
                    Notes = x.Notes,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                });
            }
        }
        public SponsorshipBenefit()
        {
        }
        public SponsorshipBenefit(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<SponsorshipBenefit>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal SponsorshipBenefit(EF.TSponsorshipBenefit row)
        {
            SetData(row);
        }
        private void SetData(EF.TSponsorshipBenefit row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<SponsorshipBenefit>();
            SponsorshipId = row.SponsorshipId;
            Title = row.Title;
            Description = row.Description;
            SeqNo = row.SeqNo;
            IsDone = (SponsorshipBenefitStatus)row.IsDone;
            DoneAt = row.DoneAt;
            DoneBy = row.DoneBy;
            ProofFileName = row.ProofFileName;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
