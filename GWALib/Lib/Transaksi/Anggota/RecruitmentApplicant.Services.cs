using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Anggota
{
    public partial class RecruitmentApplicant
    {
        private const int NOTES_LENGTH = 255;

        #region SYNC
        public static List<RecruitmentApplicant> GetDataList(int? recruitmentId = null, ApplicantStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(recruitmentId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<RecruitmentApplicant, EF.TRecruitmentApplicant>();
        }
        // Pendaftaran dari halaman publik, tanpa login (ins_by kosong).
        public static Response<RecruitmentApplicant> Apply(NewRow newrow)
        {
            return ServiceHandler.Handle(() =>
            {
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow);
                context.TRecruitmentApplicants.Add(row);
                context.SaveChanges();
                return new RecruitmentApplicant(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<RecruitmentApplicant>(msg));
        }
        // Memindah status seleksi. Accepted = mengecek kuota dan membuat m_member bila perlu (sectionId wajib untuk anggota baru).
        public Response<RecruitmentApplicant> Review(ApplicantStatus status, string? notes, string user, int? sectionId = null)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetReview(context, row, status, notes, sectionId, userId);
                context.SaveChanges();
                return new RecruitmentApplicant(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<RecruitmentApplicant>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<RecruitmentApplicant>> GetDataListAsync(int? recruitmentId = null, ApplicantStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(recruitmentId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<RecruitmentApplicant, EF.TRecruitmentApplicant>();
        }
        public static async Task<Response<RecruitmentApplicant>> ApplyAsync(NewRow newrow)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow);
                context.TRecruitmentApplicants.Add(row);
                await context.SaveChangesAsync();
                return new RecruitmentApplicant(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<RecruitmentApplicant>(msg));
        }
        public async Task<Response<RecruitmentApplicant>> ReviewAsync(ApplicantStatus status, string? notes, string user, int? sectionId = null)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetReview(context, row, status, notes, sectionId, userId);
                await context.SaveChangesAsync();
                return new RecruitmentApplicant(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<RecruitmentApplicant>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TRecruitmentApplicant> FilterQuery(int? recruitmentId, ApplicantStatus? status)
        {
            return _Querys.Where(x => (recruitmentId == null || x.RecruitmentId == recruitmentId)
                    && (status == null || x.Status == (int)status))
                .OrderBy(x => x.InsDate).ThenBy(x => x.Id);
        }
        private static EF.TRecruitmentApplicant GetRow(EF.ModelContext context, int id)
        {
            return context.TRecruitmentApplicants.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pendaftar"));
        }
        private static async Task<EF.TRecruitmentApplicant> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TRecruitmentApplicants.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pendaftar"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static EF.TRecruitmentApplicant CreateRow(EF.ModelContext context, NewRow newrow)
        {
            var recruitment = Recruitment.GetOpenRecruitment(context, newrow.RecruitmentId);
            if (!Enum.IsDefined(newrow.ApplicantCategory))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kategori Pendaftar"));
            }
            if (newrow.ApplicantCategory == MemberCategory.Umum && recruitment.AcceptPublic != (int)RecruitmentAudience.TermasukUmum)
            {
                throw new ArgumentException(ExceptionMessage.RECRUITMENT_STUDENT_ONLY);
            }
            var nim = ToNullable(newrow.Nim);
            var fullname = newrow.Fullname?.Trim() ?? string.Empty;
            var email = newrow.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            if (newrow.ApplicantCategory == MemberCategory.Mahasiswa && nim == null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "NIM"));
            }
            if (string.IsNullOrWhiteSpace(fullname))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Lengkap"));
            }
            if (!MailAddress.TryCreate(email, out var address) || address.Address != email)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Email"));
            }
            if (nim != null)
            {
                ValidateUtility.CheckDuplicateData<EF.TRecruitmentApplicant>(context, x => x.RecruitmentId == recruitment.Id && x.Nim == nim,
                    ExceptionMessage.RECRUITMENT_ALREADY_APPLIED);
            }
            ValidateUtility.CheckDuplicateData<EF.TRecruitmentApplicant>(context, x => x.RecruitmentId == recruitment.Id && x.Email == email,
                ExceptionMessage.RECRUITMENT_ALREADY_APPLIED);
            // Hubungkan ke anggota yang sudah ada bila NIM / email cocok.
            var memberId = context.MMembers.Where(x => (nim != null && x.Nim == nim) || x.Email == email).Select(x => (int?)x.Id).FirstOrDefault();

            var row = new EF.TRecruitmentApplicant
            {
                Id = SequenceUtility.GetNextId<EF.TRecruitmentApplicant>(context),
                RecruitmentId = recruitment.Id,
                MemberId = memberId,
                ApplicantCategory = (int)newrow.ApplicantCategory,
                Nim = nim,
                Fullname = fullname,
                Email = email,
                Phone = ToNullable(newrow.Phone),
                ProgramStudi = ToNullable(newrow.ProgramStudi),
                Angkatan = newrow.Angkatan,
                Institution = ToNullable(newrow.Institution),
                Motivation = ToNullable(newrow.Motivation),
                Status = (int)ApplicantStatus.Submitted,
                InsDate = DateTime.Now
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Status hanya maju; Accepted / Rejected final.
        private static void SetReview(EF.ModelContext context, EF.TRecruitmentApplicant row, ApplicantStatus status, string? notes, int? sectionId, int userId)
        {
            var current = (ApplicantStatus)row.Status;
            TransaksiHelper.ValidateStatus(current, "menilai pendaftar", ApplicantStatus.Submitted, ApplicantStatus.Screening, ApplicantStatus.Interview);
            if (!Enum.IsDefined(status) || status == ApplicantStatus.Submitted
                || (status != ApplicantStatus.Accepted && status != ApplicantStatus.Rejected && status <= current))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Pendaftar"));
            }
            if (status == ApplicantStatus.Accepted)
            {
                Accept(context, row, sectionId, userId);
            }

            row.Status = (int)status;
            row.ReviewedBy = userId;
            row.ReviewedAt = DateTime.Now;
            row.Notes = TransaksiHelper.Truncate(notes, NOTES_LENGTH) ?? row.Notes;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // Cek kuota lalu buat m_member (Anggota, bergabung hari ini, section awal = sectionId) bila pendaftar belum terhubung ke anggota.
        private static void Accept(EF.ModelContext context, EF.TRecruitmentApplicant row, int? sectionId, int userId)
        {
            var quota = context.TRecruitments.Where(x => x.Id == row.RecruitmentId).Select(x => x.Quota).Single();
            if (quota != null && context.TRecruitmentApplicants.Count(x => x.RecruitmentId == row.RecruitmentId && x.Status == (int)ApplicantStatus.Accepted) >= quota)
            {
                throw new ArgumentException(ExceptionMessage.RECRUITMENT_QUOTA_FULL);
            }
            if (row.MemberId != null)
            {
                return;
            }
            var member = Member.CreateRow(context, new Member.NewRow
            {
                MemberType = MemberType.Anggota,
                JoinedAt = DateOnly.FromDateTime(DateTime.Today),
                MemberCategory = (MemberCategory)row.ApplicantCategory,
                Nim = row.Nim,
                Fullname = row.Fullname,
                ProgramStudi = row.ProgramStudi,
                Angkatan = row.Angkatan,
                Institution = row.Institution,
                Phone = row.Phone,
                Email = row.Email,
                SectionId = sectionId ?? 0
            }, userId);
            context.MMembers.Add(member);
            row.MemberId = member.Id;
        }
        #endregion
    }
}
