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
    /// Sertifikat anggota (t_certificate). Nomor kosong = dibuat dari format CERTIFICATE, atau SRT-C/yyyyMMdd/id bila format belum diatur; nomor unik.
    /// VerificationCode acak (12 karakter, unik) untuk cek keaslian publik lewat Verify(code). Referensi wajib sesuai jenis: Peserta / Panitia /
    /// Pemateri → kegiatan; Pengurus → penugasan milik anggota; Prestasi → prestasi milik anggota; kolom yang tidak berlaku dikosongkan.
    /// Status Terbit → Dicabut (Revoke) dan bisa diterbitkan lagi (Restore). File (maksimal 5 MB) tidak dimuat di daftar (GetFile / UpdateFile).
    /// </summary>
    public partial class Certificate : BaseLib
    {
        public class NewRow
        {
            public int MemberId { get; set; }
            public CertificateType CertificateType { get; set; } = CertificateType.Peserta;
            public int? ActivityId { get; set; }
            public int? AssignmentId { get; set; }
            public int? AchievementId { get; set; }
            // Kosong = nomor otomatis.
            public string? CertificateNumber { get; set; }
            public string Title { get; set; } = null!;
            public DateOnly IssuedDate { get; set; }
            public byte[]? FileData { get; set; }
            public string? FileName { get; set; }
        }
        public record FileContent(byte[] Data, string FileName);

        public int Id { get; set; }
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public CertificateType CertificateType { get; private set; }
        public int? ActivityId { get; private set; }
        public int? AssignmentId { get; private set; }
        public int? AchievementId { get; private set; }
        public string CertificateNumber { get; private set; } = null!;
        public string Title { get; set; } = null!;
        public DateOnly IssuedDate { get; set; }
        public string VerificationCode { get; private set; } = null!;
        // Terisi = file ada.
        public string? FileName { get; private set; }
        // Hanya-baca: diubah lewat Revoke / Restore.
        public CertificateStatus Status { get; private set; }
        public DateTime InsDate { get; private set; }

        // Kolom file (blob) tidak ikut di-select.
        private static IQueryable<EF.TCertificate> _Querys
        {
            get
            {
                return DBClass.GetContext().TCertificates.Select(x => new EF.TCertificate
                {
                    Id = x.Id,
                    MemberId = x.MemberId,
                    Member = x.Member,
                    CertificateType = x.CertificateType,
                    ActivityId = x.ActivityId,
                    AssignmentId = x.AssignmentId,
                    AchievementId = x.AchievementId,
                    CertificateNumber = x.CertificateNumber,
                    Title = x.Title,
                    IssuedDate = x.IssuedDate,
                    VerificationCode = x.VerificationCode,
                    FileName = x.FileName,
                    Status = x.Status,
                    InsDate = x.InsDate
                });
            }
        }
        public Certificate()
        {
        }
        public Certificate(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Certificate>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Certificate(EF.TCertificate row)
        {
            SetData(row);
        }
        private void SetData(EF.TCertificate row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Certificate>();
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            CertificateType = (CertificateType)row.CertificateType;
            ActivityId = row.ActivityId;
            AssignmentId = row.AssignmentId;
            AchievementId = row.AchievementId;
            CertificateNumber = row.CertificateNumber;
            Title = row.Title;
            IssuedDate = row.IssuedDate;
            VerificationCode = row.VerificationCode;
            FileName = row.FileName;
            Status = (CertificateStatus)row.Status;
            InsDate = row.InsDate;
        }
    }
}
