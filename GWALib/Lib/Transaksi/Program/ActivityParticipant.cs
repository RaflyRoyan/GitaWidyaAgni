using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Program
{
    /// <summary>
    /// Peserta / petugas kegiatan (t_activity_participant), mis. PESERTA, PEMATERI, atau petugas piket. Satu anggota sekali per kegiatan.
    /// Alur status: Registered → Confirmed; Registered / Confirmed → Cancelled. Mendaftar lagi setelah Cancelled mengaktifkan baris lama.
    /// </summary>
    public partial class ActivityParticipant : BaseLib
    {
        public class NewRow
        {
            public int ActivityId { get; set; }
            public int MemberId { get; set; }
            // Kode peran bebas huruf besar, mis. PESERTA, PANITIA, PEMATERI.
            public string ParticipantRole { get; set; } = DEFAULT_ROLE;
        }

        public const string DEFAULT_ROLE = "PESERTA";

        public int Id { get; set; }
        public int ActivityId { get; private set; }
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public string ParticipantRole { get; set; } = null!;
        // Hanya-baca: diubah lewat Confirm / Cancel.
        public RegistrationStatus RegistrationStatus { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TActivityParticipant> _Querys
        {
            get
            {
                return DBClass.GetContext().TActivityParticipants.Include(x => x.Member).AsQueryable();
            }
        }
        public ActivityParticipant()
        {
        }
        public ActivityParticipant(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ActivityParticipant>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ActivityParticipant(EF.TActivityParticipant row)
        {
            SetData(row);
        }
        private void SetData(EF.TActivityParticipant row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ActivityParticipant>();
            ActivityId = row.ActivityId;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            ParticipantRole = row.ParticipantRole;
            RegistrationStatus = (RegistrationStatus)row.RegistrationStatus;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
