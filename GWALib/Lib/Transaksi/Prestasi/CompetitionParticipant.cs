using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Prestasi
{
    /// <summary>
    /// Peserta lomba (t_competition_participant), unik per (lomba, anggota). Daftar hanya saat lomba Registration dan sebelum batas pendaftaran;
    /// daftar lagi setelah Withdrawn mengaktifkan baris lama. Peran: KETUA_TIM, ANGGOTA, PEMBIMBING.
    /// Alur status: Registered → Participating → Finished; Registered / Participating → Withdrawn. Lomba Finished menandai peserta Finished.
    /// </summary>
    public partial class CompetitionParticipant : BaseLib
    {
        public class NewRow
        {
            public int CompetitionId { get; set; }
            public int MemberId { get; set; }
            public string? TeamName { get; set; }
            public string ParticipantRole { get; set; } = ROLE_ANGGOTA;
        }

        public const string ROLE_KETUA_TIM = "KETUA_TIM";
        public const string ROLE_ANGGOTA = "ANGGOTA";
        public const string ROLE_PEMBIMBING = "PEMBIMBING";

        public int Id { get; set; }
        public int CompetitionId { get; private set; }
        public string CompetitionName { get; private set; } = null!;
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public string? TeamName { get; set; }
        public string ParticipantRole { get; set; } = null!;
        // Hanya-baca: diubah lewat SetParticipating / Withdraw / penyelesaian lomba.
        public CompetitionParticipantStatus Status { get; private set; }
        public DateTime InsDate { get; private set; }

        private static IQueryable<EF.TCompetitionParticipant> _Querys
        {
            get
            {
                return DBClass.GetContext().TCompetitionParticipants.Select(x => new EF.TCompetitionParticipant
                {
                    Id = x.Id,
                    CompetitionId = x.CompetitionId,
                    Competition = new EF.TCompetition { Id = x.CompetitionId, Name = x.Competition.Name },
                    MemberId = x.MemberId,
                    Member = x.Member,
                    TeamName = x.TeamName,
                    ParticipantRole = x.ParticipantRole,
                    Status = x.Status,
                    InsDate = x.InsDate
                });
            }
        }
        public CompetitionParticipant()
        {
        }
        public CompetitionParticipant(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<CompetitionParticipant>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal CompetitionParticipant(EF.TCompetitionParticipant row)
        {
            SetData(row);
        }
        private void SetData(EF.TCompetitionParticipant row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<CompetitionParticipant>();
            CompetitionId = row.CompetitionId;
            CompetitionName = row.Competition?.Name ?? string.Empty;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            TeamName = row.TeamName;
            ParticipantRole = row.ParticipantRole;
            Status = (CompetitionParticipantStatus)row.Status;
            InsDate = row.InsDate;
        }
    }
}
