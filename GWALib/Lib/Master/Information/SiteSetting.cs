using GWALib.General;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Information
{
    /// <summary>
    /// Pengaturan situs / landing page (m_site_setting), mis. about_text, contact_email, instagram_url. Key unik (snake_case).
    /// Query tidak menarik blob gambar (lihat <see cref="ListRow"/>); gambar diambil lewat GetImage() dan diganti lewat UpdateImage().
    /// </summary>
    public partial class SiteSetting : BaseLib
    {
        public class NewRow
        {
            public string SettingGroup { get; set; } = null!;
            public string SettingKey { get; set; } = null!;
            public string? SettingValue { get; set; }
            public string? Description { get; set; }
            // Opsional: isi file gambar asli dari upload; di-resize/kompres oleh ImageUtility.
            public byte[]? SettingImage { get; set; }
        }
        public record ImageContent(byte[] Data, string ContentType);

        public int Id { get; set; }
        public string SettingGroup { get; set; } = null!;
        public string SettingKey { get; set; } = null!;
        public string? SettingValue { get; set; }
        public bool HasImage { get; private set; }
        public int? ImageSize { get; private set; }
        public string? Description { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        // Baris hasil query tanpa kolom setting_image (blob): kolom pengaturan + ukuran gambar.
        private sealed class ListRow
        {
            public EF.MSiteSetting Setting { get; init; } = null!;
            public int? ImageSize { get; init; }
        }

        EF.ModelContext _context = null!;
        private IQueryable<ListRow> _Query
        {
            get
            {
                return Project(_context.MSiteSettings);
            }
        }
        private static IQueryable<ListRow> _Querys
        {
            get
            {
                return Project(DBClass.GetContext().MSiteSettings);
            }
        }
        private static IQueryable<ListRow> Project(IQueryable<EF.MSiteSetting> query)
        {
            return query.Select(x => new ListRow
            {
                Setting = new EF.MSiteSetting
                {
                    Id = x.Id,
                    SettingGroup = x.SettingGroup,
                    SettingKey = x.SettingKey,
                    SettingValue = x.SettingValue,
                    Description = x.Description,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                },
                ImageSize = x.SettingImage == null ? null : x.SettingImage.Length
            });
        }
        public SiteSetting()
        {
            _context = DBClass.GetContext();
        }
        public SiteSetting(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<SiteSetting>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Setting.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal SiteSetting(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Setting.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal SiteSetting(EF.MSiteSetting row)
        {
            SetData(row, row.SettingImage?.Length);
        }
        private SiteSetting(ListRow row)
        {
            SetData(row);
        }
        private void SetData(ListRow row)
        {
            SetData(row.Setting, row.ImageSize);
        }
        private void SetData(EF.MSiteSetting row, int? imageSize)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<SiteSetting>();
            SettingGroup = row.SettingGroup;
            SettingKey = row.SettingKey;
            SettingValue = row.SettingValue;
            HasImage = imageSize != null;
            ImageSize = imageSize;
            Description = row.Description;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
