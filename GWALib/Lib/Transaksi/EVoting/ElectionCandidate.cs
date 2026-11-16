using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.EVoting
{
    /// <summary>
    /// Kandidat pemilihan (t_election_candidate), unik per (pemilihan, anggota) dan per nomor urut. Semua perubahan hanya saat pemilihan
    /// Draft / Registration. Anggota harus aktif; daftar lagi setelah Rejected / Withdrawn mengaktifkan baris lama.
    /// Alur: Registered → Verified / Rejected, Rejected → Verified, Registered / Verified → Withdrawn. Rejected / Withdrawn melepas nomor urut.
    /// Nomor urut boleh kosong; diisi otomatis saat pemungutan suara dibuka. Foto dinormalisasi ImageUtility dan tidak dimuat di daftar
    /// (GetPhoto / UpdatePhoto). Setiap perubahan dicatat di audit.
    /// </summary>
    public partial class ElectionCandidate : BaseLib
    {
        public class NewRow
        {
            public int ElectionId { get; set; }
            public int MemberId { get; set; }
            public int? CandidateNumber { get; set; }
            public string? Vision { get; set; }
            public string? Mission { get; set; }
            public byte[]? Photo { get; set; }
        }
        public record PhotoContent(byte[] Data, string ContentType);

        public int Id { get; set; }
        public int ElectionId { get; private set; }
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public string? Nim { get; private set; }
        public int? CandidateNumber { get; set; }
        public string? Vision { get; set; }
        public string? Mission { get; set; }
        // Hanya-baca: diubah lewat Verify / Reject / Withdraw.
        public CandidateStatus Status { get; private set; }

        // Foto (blob) tidak ikut di-select.
        private static IQueryable<EF.TElectionCandidate> _Querys
        {
            get
            {
                return DBClass.GetContext().TElectionCandidates.Select(x => new EF.TElectionCandidate
                {
                    Id = x.Id,
                    ElectionId = x.ElectionId,
                    MemberId = x.MemberId,
                    Member = new EF.MMember { Id = x.MemberId, Fullname = x.Member.Fullname, Nim = x.Member.Nim },
                    CandidateNumber = x.CandidateNumber,
                    Vision = x.Vision,
                    Mission = x.Mission,
                    Status = x.Status
                });
            }
        }
        public ElectionCandidate()
        {
        }
        public ElectionCandidate(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ElectionCandidate>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ElectionCandidate(EF.TElectionCandidate row)
        {
            SetData(row);
        }
        private void SetData(EF.TElectionCandidate row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ElectionCandidate>();
            ElectionId = row.ElectionId;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            Nim = row.Member?.Nim;
            CandidateNumber = row.CandidateNumber;
            Vision = row.Vision;
            Mission = row.Mission;
            Status = (CandidateStatus)row.Status;
        }
    }
}
