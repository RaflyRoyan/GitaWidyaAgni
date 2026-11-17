using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GWALib.Lib.Transaksi.Program
{
    public partial class Lpj
    {
        public const int MAX_DOCUMENT_BYTES = 50 * 1024 * 1024;

        #region SYNC
        public static List<Lpj> GetDataList(LpjType? type = null, LpjStatus? status = null)
        {
            return _Querys.Where(x => (type == null || x.LpjType == (int)type) && (status == null || x.Status == (int)status))
                .OrderByDescending(x => x.UpdDate ?? x.InsDate)
                .ToList().MappingData<Lpj, EF.TLpj>();
        }
        public FileContent? GetDocument()
        {
            using var context = DBClass.GetContext();
            var file = context.TLpjs.Where(x => x.Id == Id).Select(x => new { x.Document, x.DocumentName }).SingleOrDefault();
            return file?.Document == null ? null : new FileContent(file.Document, file.DocumentName ?? "LPJ.pdf");
        }
        public static Response<Lpj> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TLpjs.Add(row);
                context.SaveChanges();
                return new Lpj(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Lpj>(msg));
        }
        public Response<Lpj> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((LpjStatus)row.Status, "mengubah LPJ", LpjStatus.Draft);
                var title = Title?.Trim() ?? string.Empty;
                if (title.Length == 0)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul LPJ"));
                }
                row.Title = title;
                row.Introduction = Clean(Introduction);
                row.Purpose = Clean(Purpose);
                row.Closing = Clean(Closing);
                var kegiatan = row.LpjType == (int)LpjType.Kegiatan;
                row.Background = kegiatan ? Clean(Background) : null;
                row.ActivityKind = kegiatan ? Clean(ActivityKind) : null;
                row.ActivityForm = kegiatan ? Clean(ActivityForm) : null;
                row.VisionMission = kegiatan ? null : Clean(VisionMission);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
                context.SaveChanges();
                return new Lpj(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Lpj>(msg));
        }
        /// <summary>
        /// Draft → Final dengan PDF hasil cetak. LPJ kepengurusan: PDF juga dipasang ke serah terima dari periode ini
        /// bila serah terimanya masih Draft / Ditolak (HandoverAttached = true).
        /// </summary>
        public Response<Lpj> Finalize(byte[]? pdf, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                if (pdf == null || pdf.Length == 0)
                {
                    throw new ArgumentException(ExceptionMessage.LPJ_DOCUMENT_REQUIRED);
                }
                if (pdf.Length > MAX_DOCUMENT_BYTES)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Ukuran dokumen LPJ"));
                }
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((LpjStatus)row.Status, "memfinalkan LPJ", LpjStatus.Draft);
                var now = DateTime.Now;
                row.Status = (int)LpjStatus.Final;
                row.Document = pdf;
                row.DocumentName = FileName(row.Title);
                row.FinalizedBy = userId;
                row.FinalizedAt = now;
                row.UpdDate = now;
                row.UpdBy = userId;

                var attached = false;
                if (row.LpjType == (int)LpjType.Kepengurusan && row.PeriodId != null)
                {
                    var handover = context.TPeriodHandovers.SingleOrDefault(x => x.FromPeriodId == row.PeriodId
                        && (x.Status == (int)HandoverStatus.Draft || x.Status == (int)HandoverStatus.Ditolak));
                    if (handover != null)
                    {
                        handover.LpjDocument = pdf;
                        handover.LpjFileName = row.DocumentName;
                        handover.UpdDate = now;
                        handover.UpdBy = userId;
                        attached = true;
                    }
                }
                context.SaveChanges();
                var data = new Lpj(Reload(row.Id));
                data.HandoverAttached = attached;
                return data;
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Lpj>(msg));
        }
        // Final → Draft; PDF final dibuang (berkas yang sudah dipasang di serah terima dibiarkan).
        public Response<Lpj> Reopen(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((LpjStatus)row.Status, "membuka kembali LPJ", LpjStatus.Final);
                row.Status = (int)LpjStatus.Draft;
                row.Document = null;
                row.DocumentName = null;
                row.FinalizedBy = null;
                row.FinalizedAt = null;
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                context.SaveChanges();
                return new Lpj(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Lpj>(msg));
        }
        public Response<Lpj> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((LpjStatus)row.Status, "menghapus LPJ", LpjStatus.Draft);
                var data = new Lpj(Reload(row.Id));
                context.TLpjs.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Lpj>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TLpj Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static EF.TLpj GetRow(EF.ModelContext context, int id)
        {
            return context.TLpjs.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "LPJ"));
        }
        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static string FileName(string title)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var safe = new string(title.Select(c => invalid.Contains(c) ? '-' : c).ToArray()).Trim();
            if (safe.Length > 120)
            {
                safe = safe[..120];
            }
            return "LPJ " + safe + ".pdf";
        }
        private static EF.TLpj CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            if (!Enum.IsDefined(newrow.LpjType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis LPJ"));
            }
            var org = context.MOrganizations.AsNoTracking().FirstOrDefault();
            var orgName = org?.Name ?? "Gita Widya Agni";
            var row = new EF.TLpj
            {
                LpjType = (int)newrow.LpjType,
                Introduction = Clean(newrow.Introduction) ?? Clean(org?.Description),
                Purpose = Clean(newrow.Purpose),
                Status = (int)LpjStatus.Draft,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            if (newrow.LpjType == LpjType.Kegiatan)
            {
                var activityId = newrow.ActivityId ?? 0;
                var activity = context.TActivities.AsNoTracking().SingleOrDefault(x => x.Id == activityId)
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kegiatan"));
                if (activity.ParentActivityId != null)
                {
                    throw new ArgumentException(ExceptionMessage.LPJ_SUB_ACTIVITY);
                }
                if (context.TLpjs.Any(x => x.ActivityId == activityId))
                {
                    throw new ArgumentException(ExceptionMessage.LPJ_ACTIVITY_EXISTS);
                }
                var children = context.TActivities.Where(x => x.ParentActivityId == activityId).OrderBy(x => x.StartAt).Select(x => x.Title).ToList();
                row.ActivityId = activityId;
                row.Title = Clean(newrow.Title) ?? activity.Title;
                row.Background = Clean(newrow.Background) ?? Clean(activity.Description);
                row.ActivityKind = Clean(newrow.ActivityKind) ?? activity.Title;
                row.ActivityForm = Clean(newrow.ActivityForm) ?? (children.Count == 0 ? null : string.Join("\n", children.Select(x => "- " + x)));
                row.Closing = Clean(newrow.Closing) ?? $"Demikian laporan pertanggungjawaban {activity.Title} ini kami susun. "
                    + $"Semoga kegiatan ini memberi manfaat bagi seluruh anggota dan dapat menjadi bahan evaluasi untuk kegiatan {orgName} berikutnya.";
            }
            else
            {
                var periodId = newrow.PeriodId ?? 0;
                var period = context.MManagementPeriods.AsNoTracking().SingleOrDefault(x => x.Id == periodId)
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Periode"));
                if (context.TLpjs.Any(x => x.PeriodId == periodId))
                {
                    throw new ArgumentException(ExceptionMessage.LPJ_PERIOD_EXISTS);
                }
                var visionMission = string.Join("\n\n", new[]
                {
                    Clean(org?.Vision) == null ? null : "Visi:\n" + org!.Vision!.Trim(),
                    Clean(org?.Mission) == null ? null : "Misi:\n" + org!.Mission!.Trim()
                }.Where(x => x != null));
                row.PeriodId = periodId;
                row.Title = Clean(newrow.Title) ?? $"Kepengurusan {orgName} {period.Name}";
                row.VisionMission = Clean(newrow.VisionMission) ?? Clean(visionMission);
                row.Closing = Clean(newrow.Closing) ?? $"Demikian laporan pertanggungjawaban kepengurusan {orgName} {(period.Name.StartsWith("Periode", StringComparison.OrdinalIgnoreCase) ? period.Name : "periode " + period.Name)} ini kami susun. "
                    + "Terima kasih kepada seluruh pengurus, anggota, pelatih, alumni, dan pihak kampus atas dukungannya. "
                    + "Semoga evaluasi dalam laporan ini dapat diperbaiki oleh kepengurusan selanjutnya.";
            }
            row.Id = SequenceUtility.GetNextId<EF.TLpj>(context);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        #endregion
    }
}
