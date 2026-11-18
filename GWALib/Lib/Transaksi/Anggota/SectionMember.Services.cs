using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;

namespace GWALib.Lib.Transaksi.Anggota
{
    public partial class SectionMember
    {
        #region SYNC
        /// <summary>Riwayat section. currentOnly = hanya section saat ini (EndDate NULL). Urut anggota lalu tanggal mulai terbaru.</summary>
        public static List<SectionMember> GetDataList(int? memberId = null, int? sectionId = null, bool currentOnly = false, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(memberId, sectionId, currentOnly);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<SectionMember, EF.TSectionMember>();
        }
        /// <summary>Section aktif seorang anggota, null bila belum ada.</summary>
        public static SectionMember? GetCurrent(int memberId)
        {
            var row = _Querys.Where(x => x.MemberId == memberId && x.EndDate == null).OrderByDescending(x => x.StartDate).FirstOrDefault();
            return row == null ? null : new SectionMember(row);
        }
        /// <summary>
        /// Pindah section: menutup section aktif (EndDate = StartDate − 1 hari) lalu membuka section baru.
        /// Bila tanggal mulai sama dengan section aktif, section aktif dikoreksi (tidak menambah riwayat).
        /// Anggota tanpa section aktif langsung mendapat baris baru.
        /// </summary>
        public static Response<SectionMember> Move(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = MoveRow(context, newrow, userId);
                context.SaveChanges();
                return new SectionMember(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SectionMember>(msg));
        }
        /// <summary>Hanya catatan yang bisa diubah; tanggal mengikuti perpindahan.</summary>
        public Response<SectionMember> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                row.Note = TransaksiHelper.Truncate(TransaksiHelper.ToNullable(Note), NOTE_LENGTH);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                context.SaveChanges();
                return new SectionMember(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SectionMember>(msg));
        }
        /// <summary>
        /// Hapus satu baris riwayat (salah input). Menghapus section aktif membuka lagi section sebelumnya.
        /// Section aktif satu-satunya milik anggota Aktif tidak bisa dihapus (anggota wajib punya section).
        /// </summary>
        public Response<SectionMember> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var data = new SectionMember(Reload(row.Id));
                if (row.EndDate == null)
                {
                    var previous = context.TSectionMembers.Where(x => x.MemberId == row.MemberId && x.Id != row.Id)
                        .OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).FirstOrDefault();
                    if (previous == null)
                    {
                        var status = context.MMembers.Where(x => x.Id == row.MemberId).Select(x => x.Status).Single();
                        if (status == (int)MemberStatus.Active)
                        {
                            throw new ArgumentException("Section satu-satunya anggota aktif tidak bisa dihapus. Pindahkan ke section lain bila salah.");
                        }
                    }
                    else
                    {
                        previous.EndDate = null;
                    }
                }
                context.TSectionMembers.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<SectionMember>(msg));
        }
        #endregion

        #region HELPER
        /// <summary>Id anggota Aktif yang berada di section pada tanggal tersebut (dipakai daftar wajib presensi).</summary>
        internal static IQueryable<int> MemberIdsAt(EF.ModelContext context, int sectionId, DateOnly date)
        {
            return context.TSectionMembers
                .Where(x => x.SectionId == sectionId && x.StartDate <= date && (x.EndDate == null || x.EndDate >= date)
                    && x.Member.Status == (int)MemberStatus.Active)
                .Select(x => x.MemberId);
        }
        /// <summary>Baris section awal untuk anggota baru (dipakai Member.CreateRow). Member = navigasi agar EF menyimpan anggota dulu.</summary>
        internal static EF.TSectionMember NewHistoryRow(EF.ModelContext context, EF.MMember member, int sectionId, DateOnly startDate, string? note, int userId)
        {
            if (sectionId <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Section"));
            }
            Section.ValidateActiveSection(context, sectionId);
            return new EF.TSectionMember
            {
                Id = SequenceUtility.GetNextId<EF.TSectionMember>(context),
                SectionId = sectionId,
                Member = member,
                MemberId = member.Id,
                StartDate = startDate,
                Note = TransaksiHelper.Truncate(TransaksiHelper.ToNullable(note), NOTE_LENGTH),
                InsDate = DateTime.Now,
                InsBy = userId
            };
        }
        internal static EF.TSectionMember MoveRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            if (newrow.SectionId <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Section"));
            }
            Section.ValidateActiveSection(context, newrow.SectionId);
            Member.GetActiveMember(context, newrow.MemberId);
            var startDate = newrow.StartDate ?? DateOnly.FromDateTime(DateTime.Today);
            var note = TransaksiHelper.Truncate(TransaksiHelper.ToNullable(newrow.Note), NOTE_LENGTH);

            var current = context.TSectionMembers.Where(x => x.MemberId == newrow.MemberId && x.EndDate == null)
                .OrderByDescending(x => x.StartDate).FirstOrDefault();
            if (current != null)
            {
                if (current.SectionId == newrow.SectionId)
                {
                    throw new ArgumentException("Anggota sudah berada di section tersebut.");
                }
                if (startDate < current.StartDate)
                {
                    throw new ArgumentException($"Tanggal mulai tidak boleh sebelum section aktif dimulai ({current.StartDate:dd-MM-yyyy}).");
                }
                if (startDate == current.StartDate)
                {
                    // Koreksi section aktif yang dimulai pada tanggal yang sama.
                    current.SectionId = newrow.SectionId;
                    current.Note = note ?? current.Note;
                    current.UpdDate = DateTime.Now;
                    current.UpdBy = userId;
                    return current;
                }
                current.EndDate = startDate.AddDays(-1);
                current.UpdDate = DateTime.Now;
                current.UpdBy = userId;
            }
            var row = new EF.TSectionMember
            {
                Id = SequenceUtility.GetNextId<EF.TSectionMember>(context),
                SectionId = newrow.SectionId,
                MemberId = newrow.MemberId,
                StartDate = startDate,
                Note = note,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            context.TSectionMembers.Add(row);
            return row;
        }
        private static EF.TSectionMember Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TSectionMember> FilterQuery(int? memberId, int? sectionId, bool currentOnly)
        {
            return _Querys.Where(x => (memberId == null || x.MemberId == memberId)
                    && (sectionId == null || x.SectionId == sectionId)
                    && (!currentOnly || x.EndDate == null))
                .OrderBy(x => x.Member.Fullname).ThenByDescending(x => x.StartDate);
        }
        private static EF.TSectionMember GetRow(EF.ModelContext context, int id)
        {
            return context.TSectionMembers.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Riwayat Section"));
        }
        #endregion
    }
}
