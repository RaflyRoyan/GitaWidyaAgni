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
    /// Prestasi anggota / tim (t_achievement). Wajib anggota atau nama tim; tingkat kosong = tingkat lomba. Disorot harus publik; GetPublicList untuk landing page.
    /// Prestasi anggota mendapat poin Prestasi (MemberPoint, RefId = prestasi) dari aturan efektif tingkatnya; dihitung ulang saat diubah dan dihapus bersama prestasi.
    /// Tidak bisa dihapus bila dipakai sertifikat. File sertifikat dan foto (maksimal 5 MB) tidak dimuat di daftar (GetCertificate / GetPhoto).
    /// </summary>
    public partial class Achievement : BaseLib
    {
        public class NewRow
        {
            public int? CompetitionId { get; set; }
            public int? MemberId { get; set; }
            public string? TeamName { get; set; }
            public string Title { get; set; } = null!;
            // mis. Juara 1, Finalis, Medali Emas
            public string? RankTitle { get; set; }
            // Kosong = tingkat lomba.
            public EventLevel? Level { get; set; }
            public DateOnly AchievedDate { get; set; }
            public string? Description { get; set; }
            public PublicVisibility IsPublic { get; set; } = PublicVisibility.Public;
            public FeaturedStatus IsFeatured { get; set; } = FeaturedStatus.Biasa;
        }
        public record FileContent(byte[] Data, string FileName);

        public int Id { get; set; }
        public int? CompetitionId { get; set; }
        public int? MemberId { get; set; }
        public string? MemberName { get; private set; }
        public string? TeamName { get; set; }
        public string Title { get; set; } = null!;
        public string? RankTitle { get; set; }
        public EventLevel Level { get; set; }
        public DateOnly AchievedDate { get; set; }
        // Terisi = file sertifikat ada.
        public string? CertificateFileName { get; private set; }
        public string? Description { get; set; }
        public PublicVisibility IsPublic { get; set; }
        public FeaturedStatus IsFeatured { get; set; }
        public DateTime InsDate { get; private set; }

        // Kolom file (blob) tidak ikut di-select.
        private static IQueryable<EF.TAchievement> _Querys
        {
            get
            {
                return DBClass.GetContext().TAchievements.Select(x => new EF.TAchievement
                {
                    Id = x.Id,
                    CompetitionId = x.CompetitionId,
                    MemberId = x.MemberId,
                    Member = x.Member,
                    TeamName = x.TeamName,
                    Title = x.Title,
                    RankTitle = x.RankTitle,
                    Level = x.Level,
                    AchievedDate = x.AchievedDate,
                    CertificateFileName = x.CertificateFileName,
                    Description = x.Description,
                    IsPublic = x.IsPublic,
                    IsFeatured = x.IsFeatured,
                    InsDate = x.InsDate
                });
            }
        }
        public Achievement()
        {
        }
        public Achievement(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Achievement>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Achievement(EF.TAchievement row)
        {
            SetData(row);
        }
        private void SetData(EF.TAchievement row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Achievement>();
            CompetitionId = row.CompetitionId;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname;
            TeamName = row.TeamName;
            Title = row.Title;
            RankTitle = row.RankTitle;
            Level = (EventLevel)row.Level;
            AchievedDate = row.AchievedDate;
            CertificateFileName = row.CertificateFileName;
            Description = row.Description;
            IsPublic = (PublicVisibility)row.IsPublic;
            IsFeatured = (FeaturedStatus)row.IsFeatured;
            InsDate = row.InsDate;
        }
    }
}
