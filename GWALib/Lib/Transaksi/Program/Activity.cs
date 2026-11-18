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
    /// Kegiatan (t_activity): event, rapat, latihan, pertemuan, lomba, piket. Pusat panitia, presensi, galeri, laporan.
    /// ProgramId NULL = kegiatan insidental. ParentActivityId = kegiatan induk (mis. rapat persiapan event), maksimal 1 tingkat.
    /// Lomba wajib CompetitionId; audiens Divisi wajib DivisionId; audiens Section wajib SectionId (latihan sectional); audiens Tim Pelaksana wajib kegiatan induk.
    /// Alur status: Planned → Ongoing → Done; Planned / Ongoing → Cancelled. Disorot hanya untuk kegiatan publik.
    /// </summary>
    public partial class Activity : BaseLib
    {
        public class NewRow
        {
            public int? ProgramId { get; set; }
            public int? ParentActivityId { get; set; }
            public int? CompetitionId { get; set; }
            public ActivityType ActivityType { get; set; } = ActivityType.Event;
            public AudienceType AudienceType { get; set; } = AudienceType.SemuaAnggota;
            public int? DivisionId { get; set; }
            public int? SectionId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public string? Location { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public DateTime StartAt { get; set; }
            public DateTime? EndAt { get; set; }
            public PublicVisibility IsPublic { get; set; } = PublicVisibility.Private;
            public FeaturedStatus IsFeatured { get; set; } = FeaturedStatus.Biasa;
        }

        public int Id { get; set; }
        public int? ProgramId { get; set; }
        public string? ProgramTitle { get; private set; }
        public int? ParentActivityId { get; set; }
        public int? CompetitionId { get; set; }
        public ActivityType ActivityType { get; set; }
        public AudienceType AudienceType { get; set; }
        public int? DivisionId { get; set; }
        public int? SectionId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public PublicVisibility IsPublic { get; set; }
        public FeaturedStatus IsFeatured { get; set; }
        // Hanya-baca: diubah lewat SetStatus.
        public ProgramStatus Status { get; private set; }
        public int CreatedBy { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TActivity> _Querys
        {
            get
            {
                return DBClass.GetContext().TActivities.Include(x => x.Program).AsQueryable();
            }
        }
        public Activity()
        {
        }
        public Activity(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Activity>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Activity(EF.TActivity row)
        {
            SetData(row);
        }
        private void SetData(EF.TActivity row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Activity>();
            ProgramId = row.ProgramId;
            ProgramTitle = row.Program?.Title;
            ParentActivityId = row.ParentActivityId;
            CompetitionId = row.CompetitionId;
            ActivityType = (ActivityType)row.ActivityType;
            AudienceType = (AudienceType)row.AudienceType;
            DivisionId = row.DivisionId;
            SectionId = row.SectionId;
            Title = row.Title;
            Description = row.Description;
            Location = row.Location;
            Latitude = row.Latitude;
            Longitude = row.Longitude;
            StartAt = row.StartAt;
            EndAt = row.EndAt;
            IsPublic = (PublicVisibility)row.IsPublic;
            IsFeatured = (FeaturedStatus)row.IsFeatured;
            Status = (ProgramStatus)row.Status;
            CreatedBy = row.CreatedBy;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
