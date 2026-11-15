using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Program
{
    /// <summary>
    /// Laporan kegiatan / program (t_activity_report), minimal terkait salah satu: kegiatan atau program.
    /// Alur status: Draft → Submitted → Approved, atau Submitted → Revision → Submitted. Ubah data / file hanya saat Draft / Revision.
    /// File laporan tidak ikut dimuat (GetFile / UpdateFile).
    /// </summary>
    public partial class ActivityReport : BaseLib
    {
        public class NewRow
        {
            public int? ActivityId { get; set; }
            // Kosong + kegiatan punya program = diisi program kegiatan.
            public int? ProgramId { get; set; }
            public string Title { get; set; } = null!;
            public string? Summary { get; set; }
        }
        public record FileContent(byte[] Data, string FileName, string ContentType);

        public int Id { get; set; }
        public int? ActivityId { get; private set; }
        public int? ProgramId { get; private set; }
        public int SubmittedBy { get; private set; }
        public string Title { get; set; } = null!;
        public string? Summary { get; set; }
        // Terisi = file laporan ada.
        public string? FileName { get; private set; }
        public string? ContentType { get; private set; }
        // Hanya-baca: diubah lewat Submit / RequestRevision / Approve.
        public ActivityReportStatus Status { get; private set; }
        public DateTime? SubmittedAt { get; private set; }
        public int? ApprovedBy { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        // Kolom file (blob) tidak ikut di-select.
        private static IQueryable<EF.TActivityReport> _Querys
        {
            get
            {
                return DBClass.GetContext().TActivityReports.Select(x => new EF.TActivityReport
                {
                    Id = x.Id,
                    ActivityId = x.ActivityId,
                    ProgramId = x.ProgramId,
                    SubmittedBy = x.SubmittedBy,
                    Title = x.Title,
                    Summary = x.Summary,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    Status = x.Status,
                    SubmittedAt = x.SubmittedAt,
                    ApprovedBy = x.ApprovedBy,
                    ApprovedAt = x.ApprovedAt,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                });
            }
        }
        public ActivityReport()
        {
        }
        public ActivityReport(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ActivityReport>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ActivityReport(EF.TActivityReport row)
        {
            SetData(row);
        }
        private void SetData(EF.TActivityReport row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ActivityReport>();
            ActivityId = row.ActivityId;
            ProgramId = row.ProgramId;
            SubmittedBy = row.SubmittedBy;
            Title = row.Title;
            Summary = row.Summary;
            FileName = row.FileName;
            ContentType = row.ContentType;
            Status = (ActivityReportStatus)row.Status;
            SubmittedAt = row.SubmittedAt;
            ApprovedBy = row.ApprovedBy;
            ApprovedAt = row.ApprovedAt;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
