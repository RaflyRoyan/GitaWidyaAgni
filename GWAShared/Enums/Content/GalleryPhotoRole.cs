using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_gallery_photo.is_cover | foto sampul galeri
    /// </summary>
    public enum GalleryPhotoRole
    {
        [Description("Foto Biasa")] Biasa = 0,
        [Description("Foto Sampul")] Sampul = 1
    }
}
