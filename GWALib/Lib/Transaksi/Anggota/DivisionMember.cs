using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Anggota
{
    /// <summary>
    /// Keanggotaan anggota di divisi / tim (t_division_member), mis. anggota Humas periode 2026 atau pemain Tim Futsal.
    /// PeriodId NULL = keanggotaan tim tidak terikat periode. Satu anggota sekali per (divisi, periode); Delete = keluar (nonaktif).
    /// </summary>
    public partial class DivisionMember : BaseLib
    {
        public class NewRow
        {
            public int DivisionId { get; set; }
            public int MemberId { get; set; }
            public int? PeriodId { get; set; }
            // Kode peran bebas huruf besar, mis. KETUA, KAPTEN, ANGGOTA.
            public string MemberRole { get; set; } = DEFAULT_ROLE;
            public DateOnly? JoinedAt { get; set; }
        }

        public const string DEFAULT_ROLE = "ANGGOTA";

        public int Id { get; set; }
        public int DivisionId { get; private set; }
        public string DivisionName { get; private set; } = null!;
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public int? PeriodId { get; private set; }
        public string? PeriodName { get; private set; }
        public string MemberRole { get; set; } = null!;
        public DateOnly? JoinedAt { get; set; }
        public DateOnly? LeftAt { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TDivisionMember> _Querys
        {
            get
            {
                return DBClass.GetContext().TDivisionMembers.Include(x => x.Division).Include(x => x.Member).Include(x => x.Period).AsQueryable();
            }
        }
        public DivisionMember()
        {
        }
        public DivisionMember(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<DivisionMember>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal DivisionMember(EF.TDivisionMember row)
        {
            SetData(row);
        }
        private void SetData(EF.TDivisionMember row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<DivisionMember>();
            DivisionId = row.DivisionId;
            DivisionName = row.Division?.Name ?? string.Empty;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            PeriodId = row.PeriodId;
            PeriodName = row.Period?.Name;
            MemberRole = row.MemberRole;
            JoinedAt = row.JoinedAt;
            LeftAt = row.LeftAt;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
