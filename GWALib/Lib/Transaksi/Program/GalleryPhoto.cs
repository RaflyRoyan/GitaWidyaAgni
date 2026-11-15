using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Program
{
    /// <summary>
    /// Foto galeri (t_gallery_photo), disimpan sebagai blob. Satu foto sampul per galeri: foto pertama otomatis jadi sampul,
    /// SetCover memindah sampul, dan menghapus sampul memindahkannya ke foto berikutnya.
    /// Untuk daftar pakai <see cref="Info"/> (GetInfoList) supaya isi blob tidak ikut ditarik.
    /// </summary>
    public partial class GalleryPhoto : BaseLib
    {
        public class NewRow
        {
            public int GalleryId { get; set; }
            // Isi file asli dari upload; di-resize/kompres oleh ImageUtility.
            public byte[] Data { get; set; } = null!;
            public string? Caption { get; set; }
            // 0 = otomatis ditaruh paling akhir.
            public int Seq { get; set; }
        }

        public int Id { get; private set; }
        public int GalleryId { get; private set; }
        [JsonIgnore]
        public byte[] Photo { get; private set; } = null!;
        public string ContentType { get; private set; } = null!;
        public string? Caption { get; set; }
        public int Seq { get; set; }
        public GalleryPhotoRole IsCover { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TGalleryPhoto> _Querys
        {
            get
            {
                return DBClass.GetContext().TGalleryPhotos.AsQueryable();
            }
        }
        public GalleryPhoto()
        {
        }
        public GalleryPhoto(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<GalleryPhoto>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal GalleryPhoto(EF.TGalleryPhoto row)
        {
            SetData(row);
        }
        private void SetData(EF.TGalleryPhoto row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<GalleryPhoto>();
            GalleryId = row.GalleryId;
            Photo = row.Photo;
            ContentType = ImageUtility.GetContentType(row.Photo);
            Caption = row.Caption;
            Seq = row.Seq;
            IsCover = (GalleryPhotoRole)row.IsCover;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
