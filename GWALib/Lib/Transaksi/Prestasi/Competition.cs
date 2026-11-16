using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Prestasi
{
    /// <summary>
    /// Lomba / kompetisi (t_competition). Alur status: Upcoming → Registration → Ongoing → Finished (boleh melompat maju), dan selain Finished → Cancelled.
    /// Finished menandai peserta Registered / Participating menjadi Finished. Disorot (IsFeatured) harus publik; GetPublicList untuk landing page.
    /// Poster (gambar, maksimal 5 MB) tidak dimuat di daftar (GetPoster / UpdatePoster). Hapus hanya Upcoming yang belum dipakai data lain.
    /// </summary>
    public partial class Competition : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public string? Organizer { get; set; }
            public EventLevel Level { get; set; } = EventLevel.Kampus;
            public string? Category { get; set; }
            public string? Description { get; set; }
            public DateTime? RegistrationDeadline { get; set; }
            public DateOnly? StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
            public string? Location { get; set; }
            public string? LinkUrl { get; set; }
            public PublicVisibility IsPublic { get; set; } = PublicVisibility.Public;
            public FeaturedStatus IsFeatured { get; set; } = FeaturedStatus.Biasa;
        }

        public const int MAX_POSTER_BYTES = 5 * 1024 * 1024;

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Organizer { get; set; }
        public EventLevel Level { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public DateTime? RegistrationDeadline { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Location { get; set; }
        public string? LinkUrl { get; set; }
        public PublicVisibility IsPublic { get; set; }
        public FeaturedStatus IsFeatured { get; set; }
        // Hanya-baca: diubah lewat SetStatus.
        public CompetitionStatus Status { get; private set; }
        public DateTime InsDate { get; private set; }
        public DateTime? UpdDate { get; private set; }

        private List<CompetitionParticipant>? ParticipantList;
        private List<CompetitionMaterial>? MaterialList;

        // Kolom poster (blob) tidak ikut di-select.
        private static IQueryable<EF.TCompetition> _Querys
        {
            get
            {
                return DBClass.GetContext().TCompetitions.Select(x => new EF.TCompetition
                {
                    Id = x.Id,
                    Name = x.Name,
                    Organizer = x.Organizer,
                    Level = x.Level,
                    Category = x.Category,
                    Description = x.Description,
                    RegistrationDeadline = x.RegistrationDeadline,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Location = x.Location,
                    LinkUrl = x.LinkUrl,
                    IsPublic = x.IsPublic,
                    IsFeatured = x.IsFeatured,
                    Status = x.Status,
                    InsDate = x.InsDate,
                    UpdDate = x.UpdDate
                });
            }
        }
        public Competition()
        {
        }
        public Competition(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Competition>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Competition(EF.TCompetition row)
        {
            SetData(row);
        }
        private void SetData(EF.TCompetition row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Competition>();
            Name = row.Name;
            Organizer = row.Organizer;
            Level = (EventLevel)row.Level;
            Category = row.Category;
            Description = row.Description;
            RegistrationDeadline = row.RegistrationDeadline;
            StartDate = row.StartDate;
            EndDate = row.EndDate;
            Location = row.Location;
            LinkUrl = row.LinkUrl;
            IsPublic = (PublicVisibility)row.IsPublic;
            IsFeatured = (FeaturedStatus)row.IsFeatured;
            Status = (CompetitionStatus)row.Status;
            InsDate = row.InsDate;
            UpdDate = row.UpdDate;
        }
    }
}
