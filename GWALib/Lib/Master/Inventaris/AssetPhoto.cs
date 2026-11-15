using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Inventaris
{
    /// <summary>
    /// Foto aset (m_asset_photo), disimpan sebagai blob. Key: (AssetId, SeqPhoto).
    /// Untuk daftar pakai <see cref="Info"/> (GetInfoList) supaya isi blob tidak ikut ditarik.
    /// </summary>
    public partial class AssetPhoto : BaseLib
    {
        public class NewRow
        {
            public int AssetId { get; set; }
            // Isi file asli dari upload; di-resize/kompres oleh ImageUtility.
            public byte[] Data { get; set; } = null!;
        }

        public int AssetId { get; private set; }
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

        private const char KEY_SEPARATOR = '|';

        private static IQueryable<EF.MAssetPhoto> _Querys
        {
            get
            {
                return DBClass.GetContext().MAssetPhotos.AsQueryable();
            }
        }
        public AssetPhoto()
        {
        }
        public AssetPhoto(string encKey)
        {
            var key = encKey.DecryptString<AssetPhoto>().Split(KEY_SEPARATOR);
            if (key.Length == 2)
            {
                var assetId = Convert.ToInt32(key[0]);
                var seqPhoto = Convert.ToInt32(key[1]);
                var row = _Querys.SingleOrDefault(x => x.AssetId == assetId && x.SeqPhoto == seqPhoto);
                if (row != null)
                {
                    SetData(row);
                }
            }
        }
        internal AssetPhoto(int assetId, int seqPhoto)
        {
            var row = _Querys.SingleOrDefault(x => x.AssetId == assetId && x.SeqPhoto == seqPhoto);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal AssetPhoto(EF.MAssetPhoto row)
        {
            SetData(row);
        }
        private void SetData(EF.MAssetPhoto row)
        {
            AssetId = row.AssetId;
            SeqPhoto = row.SeqPhoto;
            EncDecrKey = string.Join(KEY_SEPARATOR, row.AssetId, row.SeqPhoto).EncryptString<AssetPhoto>();
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
