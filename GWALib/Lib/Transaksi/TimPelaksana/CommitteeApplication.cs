using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.TimPelaksana
{
    /// <summary>
    /// Lamaran anggota menjadi tim pelaksana (t_committee_application), dengan pilihan sie 1 (wajib) dan 2 (opsional).
    /// Alur status: Daftar → Diterima / Ditolak (Review) atau Mundur (Withdraw). Melamar lagi setelah Mundur mengaktifkan baris lama.
    /// Diterima mengecek kuota dan menambahkan pelamar ke <see cref="CommitteeMember"/> pada sie yang dipilih reviewer.
    /// </summary>
    public partial class CommitteeApplication : BaseLib
    {
        public class NewRow
        {
            public int RecruitmentId { get; set; }
            public int MemberId { get; set; }
            public int SectionChoice1Id { get; set; }
            public int? SectionChoice2Id { get; set; }
            public string? Reason { get; set; }
            // Opsional: jawaban formulir tambahan (t_form_response).
            public int? FormResponseId { get; set; }
        }

        public int Id { get; private set; }
        public int RecruitmentId { get; private set; }
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public int SectionChoice1Id { get; private set; }
        public string SectionChoice1Name { get; private set; } = null!;
        public int? SectionChoice2Id { get; private set; }
        public string? SectionChoice2Name { get; private set; }
        public string? Reason { get; private set; }
        public int? FormResponseId { get; private set; }
        public CommitteeApplicationStatus Status { get; private set; }
        public int? ReviewedBy { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public string? ReviewNote { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TCommitteeApplication> _Querys
        {
            get
            {
                return DBClass.GetContext().TCommitteeApplications
                    .Include(x => x.Member).Include(x => x.SectionChoice1).Include(x => x.SectionChoice2).AsQueryable();
            }
        }
        public CommitteeApplication()
        {
        }
        public CommitteeApplication(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<CommitteeApplication>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal CommitteeApplication(EF.TCommitteeApplication row)
        {
            SetData(row);
        }
        private void SetData(EF.TCommitteeApplication row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<CommitteeApplication>();
            RecruitmentId = row.RecruitmentId;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            SectionChoice1Id = row.SectionChoice1Id;
            SectionChoice1Name = row.SectionChoice1?.Name ?? string.Empty;
            SectionChoice2Id = row.SectionChoice2Id;
            SectionChoice2Name = row.SectionChoice2?.Name;
            Reason = row.Reason;
            FormResponseId = row.FormResponseId;
            Status = (CommitteeApplicationStatus)row.Status;
            ReviewedBy = row.ReviewedBy;
            ReviewedAt = row.ReviewedAt;
            ReviewNote = row.ReviewNote;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
