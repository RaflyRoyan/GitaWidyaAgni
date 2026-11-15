using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Anggota
{
    /// <summary>
    /// Foto anggota (m_member_photo), disimpan sebagai blob.
    /// Untuk daftar pakai <see cref="Info"/> (GetInfoList) supaya isi blob tidak ikut ditarik.
    /// </summary>
    public partial class MemberPhoto : BaseLib
    {
        public class NewRow
        {
            public int MemberId { get; set; }
            // Isi file asli dari upload; di-resize/kompres oleh ImageUtility.
            public byte[] Data { get; set; } = null!;
        }

        public int Id { get; private set; }
        public int MemberId { get; private set; }
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

        private static IQueryable<EF.MMemberPhoto> _Querys
        {
            get
            {
                return DBClass.GetContext().MMemberPhotos.AsQueryable();
            }
        }
        public MemberPhoto()
        {
        }
        public MemberPhoto(string encKey)
        {
            var key = encKey.DecryptString<MemberPhoto>();
            var id = Convert.ToInt32(key);
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal MemberPhoto(int id)
        {
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal MemberPhoto(EF.MMemberPhoto row)
        {
            SetData(row);
        }
        private void SetData(EF.MMemberPhoto row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<MemberPhoto>();
            MemberId = row.MemberId;
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
