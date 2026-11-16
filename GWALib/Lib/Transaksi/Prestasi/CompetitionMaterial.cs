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
    /// Materi persiapan lomba (t_competition_material). CompetitionId kosong = materi umum. File wajib file (maksimal 20 MB); Link wajib URL;
    /// Video boleh URL atau file. Visibility Public / Member / Participant (peserta lomba): GetDataList menerima visibilitas yang boleh dilihat.
    /// File tidak dimuat di daftar (GetFile / UpdateFile). Hapus permanen.
    /// </summary>
    public partial class CompetitionMaterial : BaseLib
    {
        public class NewRow
        {
            public int? CompetitionId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public MaterialType MaterialType { get; set; } = MaterialType.File;
            public byte[]? FileData { get; set; }
            public string? FileName { get; set; }
            public string? ContentType { get; set; }
            public string? Url { get; set; }
            public MaterialVisibility Visibility { get; set; } = MaterialVisibility.Member;
        }
        public record FileContent(byte[] Data, string FileName, string? ContentType);

        public int Id { get; set; }
        public int? CompetitionId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public MaterialType MaterialType { get; set; }
        // Terisi = file ada.
        public string? FileName { get; private set; }
        public string? Url { get; set; }
        public MaterialVisibility Visibility { get; set; }
        public int UploadedBy { get; private set; }
        public DateTime InsDate { get; private set; }

        // Kolom file (blob) tidak ikut di-select.
        private static IQueryable<EF.TCompetitionMaterial> _Querys
        {
            get
            {
                return DBClass.GetContext().TCompetitionMaterials.Select(x => new EF.TCompetitionMaterial
                {
                    Id = x.Id,
                    CompetitionId = x.CompetitionId,
                    Title = x.Title,
                    Description = x.Description,
                    MaterialType = x.MaterialType,
                    FileName = x.FileName,
                    Url = x.Url,
                    Visibility = x.Visibility,
                    UploadedBy = x.UploadedBy,
                    InsDate = x.InsDate
                });
            }
        }
        public CompetitionMaterial()
        {
        }
        public CompetitionMaterial(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<CompetitionMaterial>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal CompetitionMaterial(EF.TCompetitionMaterial row)
        {
            SetData(row);
        }
        private void SetData(EF.TCompetitionMaterial row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<CompetitionMaterial>();
            CompetitionId = row.CompetitionId;
            Title = row.Title;
            Description = row.Description;
            MaterialType = (MaterialType)row.MaterialType;
            FileName = row.FileName;
            Url = row.Url;
            Visibility = (MaterialVisibility)row.Visibility;
            UploadedBy = row.UploadedBy;
            InsDate = row.InsDate;
        }
    }
}
