using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Organisasi
{
    /// <summary>Organisasi › Jabatan: master jabatan pengurus (m_position). Hapus = nonaktifkan.</summary>
    public class JabatanVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Method.GetDataList().ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public bool Inti { get; set; }
            public string? Description { get; set; }
            public int SortOrder { get; set; }
            public int JumlahPemegang { get; set; }
            public bool Aktif { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> LevelList { get; set; } = GwaVM.Opsi<PositionLevel>();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }

        public class Edit : Add
        {
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<Position>(encKey, "Jabatan");
                Row = new NewRow { Name = obj.Name, IsCore = (int)obj.IsCore, Description = obj.Description, SortOrder = obj.SortOrder, IsActive = (int)obj.IsActive };
            }
        }

        public class NewRow
        {
            [Required(ErrorMessage = "Nama jabatan harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Nama Jabatan")]
            public string? Name { get; set; }

            [Display(Name = "Tingkat")]
            public int IsCore { get; set; } = (int)PositionLevel.Biasa;

            [StringLength(255, ErrorMessage = "Keterangan maksimal 255 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Range(0, 9999, ErrorMessage = "Urutan 0 - 9999")]
            [Display(Name = "Urutan Tampil")]
            public int SortOrder { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        public class Method
        {
            public static IEnumerable<Row> GetDataList()
            {
                var holders = ManagementAssignment.GetDataList(isCurrent: AssignmentPeriod.Berjalan)
                    .GroupBy(x => x.PositionId).ToDictionary(g => g.Key, g => g.Count());
                return Position.GetDataList(FilterAktif.Semua).Select(x => new Row
                {
                    EncDecrKey = x.EncDecrKey,
                    Name = x.Name,
                    Inti = x.IsCore == PositionLevel.Inti,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    JumlahPemegang = holders.TryGetValue(x.Id, out var n) ? n : 0,
                    Aktif = x.IsActive == ActiveStatus.Aktif
                }).OrderBy(x => x.SortOrder).ThenBy(x => x.Name);
            }

            public static void Simpan(NewRow input, string user)
            {
                Position.Save(new Position.NewRow
                {
                    Name = input.Name ?? "",
                    IsCore = (PositionLevel)input.IsCore,
                    Description = input.Description,
                    SortOrder = input.SortOrder
                }, user).Ok();
            }

            public static void Update(string encKey, NewRow input, string user)
            {
                var obj = LibExtensions.Load<Position>(encKey, "Jabatan");
                obj.Name = input.Name ?? "";
                obj.IsCore = (PositionLevel)input.IsCore;
                obj.Description = input.Description;
                obj.SortOrder = input.SortOrder;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }

            public static void Nonaktifkan(string? encKey, string user)
            {
                LibExtensions.Load<Position>(encKey, "Jabatan").Delete(user).Ok();
            }
        }
    }
}
