using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.TimPelaksana
{
    /// <summary>
    /// Open recruitment tim pelaksana kegiatan (t_committee_recruitment), satu per kegiatan.
    /// Alur status: Draft → Buka → Tutup (Tutup bisa dibuka lagi bila tanggal tutup masih di depan).
    /// Lamaran (<see cref="CommitteeApplication"/>) hanya diterima saat Buka dan di antara OpenAt - CloseAt; Quota membatasi lamaran yang diterima.
    /// </summary>
    public partial class CommitteeRecruitment : BaseLib
    {
        public class NewRow
        {
            public int ActivityId { get; set; }
            public DateTime OpenAt { get; set; }
            public DateTime CloseAt { get; set; }
            public int? Quota { get; set; }
            // Opsional: formulir pertanyaan tambahan (t_form).
            public int? FormId { get; set; }
        }

        public int Id { get; set; }
        public int ActivityId { get; private set; }
        public DateTime OpenAt { get; set; }
        public DateTime CloseAt { get; set; }
        public int? Quota { get; set; }
        public int? FormId { get; set; }
        // Hanya-baca: diubah lewat Open / Close.
        public CommitteeRecruitmentStatus Status { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TCommitteeRecruitment> _Querys
        {
            get
            {
                return DBClass.GetContext().TCommitteeRecruitments.AsQueryable();
            }
        }
        public CommitteeRecruitment()
        {
        }
        public CommitteeRecruitment(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<CommitteeRecruitment>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal CommitteeRecruitment(EF.TCommitteeRecruitment row)
        {
            SetData(row);
        }
        private void SetData(EF.TCommitteeRecruitment row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<CommitteeRecruitment>();
            ActivityId = row.ActivityId;
            OpenAt = row.OpenAt;
            CloseAt = row.CloseAt;
            Quota = row.Quota;
            FormId = row.FormId;
            Status = (CommitteeRecruitmentStatus)row.Status;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
