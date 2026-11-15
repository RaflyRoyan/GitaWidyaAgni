using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Anggota
{
    /// <summary>
    /// Open recruitment anggota baru (t_recruitment). Alur status: Draft → Open → Closed.
    /// Pendaftaran (<see cref="RecruitmentApplicant"/>) hanya diterima saat Open dan di antara OpenDate - CloseDate.
    /// Quota (opsional) membatasi jumlah pendaftar yang diterima.
    /// </summary>
    public partial class Recruitment : BaseLib
    {
        public class NewRow
        {
            public int? PeriodId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public DateTime OpenDate { get; set; }
            public DateTime CloseDate { get; set; }
            public int? Quota { get; set; }
            public RecruitmentAudience AcceptPublic { get; set; } = RecruitmentAudience.HanyaMahasiswa;
        }

        public int Id { get; set; }
        public int? PeriodId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime CloseDate { get; set; }
        public int? Quota { get; set; }
        public RecruitmentAudience AcceptPublic { get; set; }
        // Hanya-baca: diubah lewat Open / Close.
        public RecruitmentStatus Status { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<RecruitmentApplicant>? ApplicantList;

        private static IQueryable<EF.TRecruitment> _Querys
        {
            get
            {
                return DBClass.GetContext().TRecruitments.AsQueryable();
            }
        }
        public Recruitment()
        {
        }
        public Recruitment(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Recruitment>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Recruitment(EF.TRecruitment row)
        {
            SetData(row);
        }
        private void SetData(EF.TRecruitment row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Recruitment>();
            PeriodId = row.PeriodId;
            Title = row.Title;
            Description = row.Description;
            OpenDate = row.OpenDate;
            CloseDate = row.CloseDate;
            Quota = row.Quota;
            AcceptPublic = (RecruitmentAudience)row.AcceptPublic;
            Status = (RecruitmentStatus)row.Status;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
