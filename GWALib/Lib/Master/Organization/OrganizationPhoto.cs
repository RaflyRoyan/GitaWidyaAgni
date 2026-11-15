using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Organization
{
    /// <summary>
    /// Foto organisasi (m_organization_photo), disimpan sebagai blob. Key: SeqPhoto, diambil dari m_sequence.
    /// Untuk daftar pakai <see cref="Info"/> (GetInfoList) supaya isi blob tidak ikut ditarik.
    /// </summary>
    public partial class OrganizationPhoto : BaseLib
    {
        public class NewRow
        {
            // Isi file asli dari upload; di-resize/kompres oleh ImageUtility.
            public byte[] Data { get; set; } = null!;
        }

        public int SeqPhoto { get; private set; }
        [JsonIgnore]
        public byte[] Photo { get; private set; } = null!;
        public string ContentType { get; private set; } = null!;
        public int FileSize { get; private set; }
        // Hash isi foto, berubah setiap foto diganti: dipakai untuk cache-buster URL (?v=) dan ETag.
        public string Version { get; private set; } = null!;
        public DateTime InsDate { get; private set; }
        public int InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.MOrganizationPhoto> _Querys
        {
            get
            {
                return DBClass.GetContext().MOrganizationPhotos.AsQueryable();
            }
        }
        public OrganizationPhoto()
        {
        }
        public OrganizationPhoto(string encKey)
        {
            var seqPhoto = Convert.ToInt32(encKey.DecryptString<OrganizationPhoto>());
            var row = _Querys.SingleOrDefault(x => x.SeqPhoto == seqPhoto);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal OrganizationPhoto(int seqPhoto)
        {
            var row = _Querys.SingleOrDefault(x => x.SeqPhoto == seqPhoto);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal OrganizationPhoto(EF.MOrganizationPhoto row)
        {
            SetData(row);
        }
        private void SetData(EF.MOrganizationPhoto row)
        {
            SeqPhoto = row.SeqPhoto;
            EncDecrKey = row.SeqPhoto.ToString().EncryptString<OrganizationPhoto>();
            Photo = row.Photo;
            ContentType = ImageUtility.GetContentType(row.Photo);
            FileSize = row.Photo.Length;
            Version = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(row.Photo), 0, 8).ToLowerInvariant();
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
