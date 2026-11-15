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
    /// Album galeri foto (t_gallery), opsional terkait kegiatan. Alur status: Draft → Published → Archived (Archived bisa diterbitkan lagi).
    /// Terbit membutuhkan minimal satu foto. Disorot hanya untuk galeri publik. Delete hanya Draft (foto ikut terhapus).
    /// </summary>
    public partial class Gallery : BaseLib
    {
        public class NewRow
        {
            public int? ActivityId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            // Kosong + kegiatan diisi = tanggal mulai kegiatan.
            public DateOnly? EventDate { get; set; }
            public PublicVisibility IsPublic { get; set; } = PublicVisibility.Public;
            public FeaturedStatus IsFeatured { get; set; } = FeaturedStatus.Biasa;
        }

        public int Id { get; set; }
        public int? ActivityId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateOnly? EventDate { get; set; }
        public PublicVisibility IsPublic { get; set; }
        public FeaturedStatus IsFeatured { get; set; }
        // Hanya-baca: diubah lewat Publish / Archive.
        public PublicationStatus Status { get; private set; }
        public DateTime? PublishedAt { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<GalleryPhoto.Info>? PhotoList;

        private static IQueryable<EF.TGallery> _Querys
        {
            get
            {
                return DBClass.GetContext().TGalleries.AsQueryable();
            }
        }
        public Gallery()
        {
        }
        public Gallery(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Gallery>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Gallery(EF.TGallery row)
        {
            SetData(row);
        }
        private void SetData(EF.TGallery row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Gallery>();
            ActivityId = row.ActivityId;
            Title = row.Title;
            Description = row.Description;
            EventDate = row.EventDate;
            IsPublic = (PublicVisibility)row.IsPublic;
            IsFeatured = (FeaturedStatus)row.IsFeatured;
            Status = (PublicationStatus)row.Status;
            PublishedAt = row.PublishedAt;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
