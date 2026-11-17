using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GWALib.Lib.Transaksi.Program
{
    /// <summary>
    /// Laporan pertanggungjawaban (t_lpj): LPJ Kegiatan (satu per kegiatan induk) atau LPJ Kepengurusan (satu per periode).
    /// Tabel ini hanya menyimpan bagian naratif yang diketik pengurus; data lain (jadwal, hasil, anggaran, program kerja,
    /// inventaris, susunan pengurus, dokumentasi) diambil langsung dari modul lain lewat GetKegiatanData / GetKepengurusanData.
    /// Draft → Final (Finalize menyimpan PDF; LPJ kepengurusan juga dipasang ke serah terima periode yang masih Draft / Ditolak),
    /// Final → Draft (Reopen). Ubah dan hapus hanya saat Draft. Dokumen tidak ikut dimuat di daftar (GetDocument).
    /// </summary>
    public partial class Lpj : BaseLib
    {
        public class NewRow
        {
            public LpjType LpjType { get; set; }
            public int? ActivityId { get; set; }
            public int? PeriodId { get; set; }
            /// <summary>Kosong = judul kegiatan / "Kepengurusan {organisasi} {periode}".</summary>
            public string? Title { get; set; }
            /// <summary>Bagian naratif yang kosong diisi awal dari profil organisasi / data kegiatan.</summary>
            public string? Introduction { get; set; }
            public string? Background { get; set; }
            public string? ActivityKind { get; set; }
            public string? Purpose { get; set; }
            public string? ActivityForm { get; set; }
            public string? VisionMission { get; set; }
            public string? Closing { get; set; }
        }

        public record FileContent(byte[] Data, string FileName);

        public int Id { get; set; }
        public LpjType LpjType { get; private set; }
        public int? ActivityId { get; private set; }
        public string? ActivityTitle { get; private set; }
        public int? PeriodId { get; private set; }
        public string? PeriodName { get; private set; }
        public string Title { get; set; } = null!;
        public string? Introduction { get; set; }
        public string? Background { get; set; }
        public string? ActivityKind { get; set; }
        public string? Purpose { get; set; }
        public string? ActivityForm { get; set; }
        public string? VisionMission { get; set; }
        public string? Closing { get; set; }
        // Hanya-baca: diubah lewat Finalize / Reopen.
        public LpjStatus Status { get; private set; }
        public string? DocumentName { get; private set; }
        public int? FinalizedBy { get; private set; }
        public DateTime? FinalizedAt { get; private set; }
        /// <summary>Hasil Finalize: PDF juga dipasang sebagai berkas LPJ serah terima periode.</summary>
        public bool HandoverAttached { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TLpj> _Querys
        {
            get
            {
                // Tanpa blob dokumen.
                return DBClass.GetContext().TLpjs.AsNoTracking().Select(x => new EF.TLpj
                {
                    Id = x.Id,
                    LpjType = x.LpjType,
                    ActivityId = x.ActivityId,
                    PeriodId = x.PeriodId,
                    Title = x.Title,
                    Introduction = x.Introduction,
                    Background = x.Background,
                    ActivityKind = x.ActivityKind,
                    Purpose = x.Purpose,
                    ActivityForm = x.ActivityForm,
                    VisionMission = x.VisionMission,
                    Closing = x.Closing,
                    Status = x.Status,
                    DocumentName = x.DocumentName,
                    FinalizedBy = x.FinalizedBy,
                    FinalizedAt = x.FinalizedAt,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy,
                    Activity = x.Activity == null ? null : new EF.TActivity { Id = x.Activity.Id, Title = x.Activity.Title, StartAt = x.Activity.StartAt },
                    Period = x.Period == null ? null : new EF.MManagementPeriod { Id = x.Period.Id, Name = x.Period.Name }
                });
            }
        }
        public Lpj()
        {
        }
        public Lpj(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Lpj>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Lpj(EF.TLpj row)
        {
            SetData(row);
        }
        private void SetData(EF.TLpj row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Lpj>();
            LpjType = (LpjType)row.LpjType;
            ActivityId = row.ActivityId;
            ActivityTitle = row.Activity?.Title;
            PeriodId = row.PeriodId;
            PeriodName = row.Period?.Name;
            Title = row.Title;
            Introduction = row.Introduction;
            Background = row.Background;
            ActivityKind = row.ActivityKind;
            Purpose = row.Purpose;
            ActivityForm = row.ActivityForm;
            VisionMission = row.VisionMission;
            Closing = row.Closing;
            Status = (LpjStatus)row.Status;
            DocumentName = row.DocumentName;
            FinalizedBy = row.FinalizedBy;
            FinalizedAt = row.FinalizedAt;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
