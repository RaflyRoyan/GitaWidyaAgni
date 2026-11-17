using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Inventaris;
using GWALib.Lib.Transaksi.Inventaris;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Inventaris
{
    public partial class AsetVM
    {
        // ================= Kategori
        public class ListKategori
        {
            public List<KategoriRow> Rows { get; set; } = AssetCategory.GetDataList(FilterAktif.Semua).Select(x => new KategoriRow
            {
                EncDecrKey = x.EncDecrKey,
                Name = x.Name,
                Description = x.Description,
                JumlahAset = x.GetAssets(true).Count,
                Aktif = x.IsActive == ActiveStatus.Aktif
            }).OrderBy(x => x.Name).ToList();
        }

        public class KategoriRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public int JumlahAset { get; set; }
            public bool Aktif { get; set; }
        }

        public class AddKategori
        {
            public NewKategori Row { get; set; } = new();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }

        public class EditKategori : AddKategori
        {
            public EditKategori(string encKey)
            {
                var obj = LibExtensions.Load<AssetCategory>(encKey, "Kategori aset");
                Row = new NewKategori { Name = obj.Name, Description = obj.Description, IsActive = (int)obj.IsActive };
            }
        }

        public class NewKategori
        {
            [Required(ErrorMessage = "Nama kategori harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Nama Kategori")]
            public string? Name { get; set; }

            [StringLength(255, ErrorMessage = "Keterangan maksimal 255 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        // ================= Perawatan
        public class ListPerawatan
        {
            public List<PerawatanRow> Rows { get; set; } = MaintenanceLog.GetDataList().Select(x => new PerawatanRow
            {
                EncDecrKey = x.EncDecrKey,
                Aset = x.AssetName,
                UnitCode = x.UnitCode,
                MaintenanceDate = x.MaintenanceDate.ToDateTime(TimeOnly.MinValue),
                Description = x.Description,
                Vendor = x.Vendor,
                Cost = x.Cost,
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status),
                FinishedAt = x.FinishedAt,
                Dibayar = x.TransactionId.HasValue
            }).OrderBy(x => x.Status).ThenByDescending(x => x.MaintenanceDate).ToList();
        }

        public class PerawatanRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Aset { get; set; } = "";
            public string? UnitCode { get; set; }
            public DateTime MaintenanceDate { get; set; }
            public string Description { get; set; } = "";
            public string? Vendor { get; set; }
            public decimal Cost { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public DateTime? FinishedAt { get; set; }
            public bool Dibayar { get; set; }
        }

        public class AddPerawatan
        {
            public NewPerawatan Row { get; set; } = new() { MaintenanceDate = DateTime.Today };
            public List<GwaVM.OpsiItem> AsetOpsi { get; set; } = OpsiAset();
        }

        public class EditPerawatan : AddPerawatan
        {
            public string Aset { get; set; } = "";
            public EditPerawatan(string encKey)
            {
                var obj = LibExtensions.Load<MaintenanceLog>(encKey, "Perawatan");
                Aset = obj.AssetName + (obj.UnitCode != null ? " · " + obj.UnitCode : "");
                Row = new NewPerawatan
                {
                    AssetId = obj.AssetId,
                    MaintenanceDate = obj.MaintenanceDate.ToDateTime(TimeOnly.MinValue),
                    Description = obj.Description,
                    Vendor = obj.Vendor,
                    Cost = obj.Cost
                };
            }
        }

        public class NewPerawatan
        {
            [Required(ErrorMessage = "Pilih aset")]
            [Display(Name = "Aset")]
            public int? AssetId { get; set; }

            [StringLength(20, ErrorMessage = "Kode unit maksimal 20 karakter")]
            [Display(Name = "Kode Unit (aset per unit)")]
            public string? UnitCode { get; set; }

            [Required(ErrorMessage = "Tanggal harus diisi")]
            [Display(Name = "Tanggal")]
            public DateTime? MaintenanceDate { get; set; }

            [Required(ErrorMessage = "Uraian harus diisi")]
            [StringLength(255, ErrorMessage = "Uraian maksimal 255 karakter")]
            [Display(Name = "Uraian Perawatan")]
            public string? Description { get; set; }

            [StringLength(150, ErrorMessage = "Vendor maksimal 150 karakter")]
            [Display(Name = "Vendor / Tukang")]
            public string? Vendor { get; set; }

            [Range(0, 999999999999, ErrorMessage = "Biaya tidak boleh negatif")]
            [Display(Name = "Biaya (Rp)")]
            public decimal Cost { get; set; }
        }

        // Selesai perawatan: kondisi baru + (opsional) akun & kategori untuk transaksi biaya.
        public class Selesai
        {
            public string Judul { get; set; } = "";
            public decimal Cost { get; set; }
            public SelesaiInput Row { get; set; } = new();
            public List<GwaVM.OpsiItem> KondisiOpsi { get; set; } = GwaVM.Opsi<ItemCondition>(ItemCondition.Hilang);
            public List<GwaVM.OpsiItem> AkunOpsi { get; set; } = Opsi.Akun();
            public List<GwaVM.OpsiItem> KategoriOpsi { get; set; } = Opsi.Kategori(CashFlowType.Expense);
            public Selesai(string encKey)
            {
                var obj = LibExtensions.Load<MaintenanceLog>(encKey, "Perawatan");
                Judul = obj.AssetName + (obj.UnitCode != null ? " · " + obj.UnitCode : "") + " — " + obj.Description;
                Cost = obj.Cost;
                Row.Key = obj.EncDecrKey;
            }
        }

        public class SelesaiInput
        {
            public string Key { get; set; } = "";

            [Display(Name = "Kondisi Setelah Perawatan")]
            public int? Condition { get; set; }

            [Display(Name = "Bayar dari Akun")]
            public int? AccountId { get; set; }

            [Display(Name = "Kategori Pengeluaran")]
            public int? CategoryId { get; set; }
        }

        // ================= Inspeksi
        public class ListInspeksi
        {
            public List<InspeksiRow> Rows { get; set; }
            public ListInspeksi()
            {
                var unit = AssetUnit.GetDataList().ToDictionary(x => x.Id, x => x.UnitCode);
                Rows = AssetInspection.GetDataList().Select(x => new InspeksiRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Aset = x.AssetName,
                    UnitCode = x.AssetUnitId.HasValue && unit.TryGetValue(x.AssetUnitId.Value, out var u) ? u : null,
                    InspectedAt = x.InspectedAt,
                    Kondisi = EnumHelper.Text(x.ItemCondition),
                    Notes = x.Notes,
                    DariPengembalian = x.BorrowItemId.HasValue
                }).OrderByDescending(x => x.InspectedAt).ToList();
            }
        }

        public class InspeksiRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Aset { get; set; } = "";
            public string? UnitCode { get; set; }
            public DateTime InspectedAt { get; set; }
            public string Kondisi { get; set; } = "";
            public string? Notes { get; set; }
            public bool DariPengembalian { get; set; }
        }

        public class AddInspeksi
        {
            public NewInspeksi Row { get; set; } = new() { InspectedAt = DateTime.Now };
            public List<GwaVM.OpsiItem> AsetOpsi { get; set; } = OpsiAset();
            public List<GwaVM.OpsiItem> KondisiOpsi { get; set; } = GwaVM.Opsi<ItemCondition>();
        }

        public class NewInspeksi
        {
            [Required(ErrorMessage = "Pilih aset")]
            [Display(Name = "Aset")]
            public int? AssetId { get; set; }

            [StringLength(20, ErrorMessage = "Kode unit maksimal 20 karakter")]
            [Display(Name = "Kode Unit (aset per unit)")]
            public string? UnitCode { get; set; }

            [Display(Name = "Kondisi")]
            public int ItemCondition { get; set; } = (int)GWAShared.Enums.ItemCondition.Baik;

            [Display(Name = "Waktu Inspeksi")]
            public DateTime? InspectedAt { get; set; }

            [StringLength(500, ErrorMessage = "Catatan maksimal 500 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }
        }

        // ================= Riwayat mutasi
        public class ListMutasi
        {
            public List<MutasiRow> Rows { get; set; } = AssetMovement.GetDataList().Select(x => new MutasiRow
            {
                Aset = x.AssetName,
                UnitCode = x.UnitCode,
                Jenis = EnumHelper.Text(x.MovementType),
                Quantity = x.Quantity,
                MovementDate = x.MovementDate,
                Referensi = x.ReferenceType == null ? null : x.ReferenceType + (x.ReferenceId.HasValue ? " #" + x.ReferenceId : ""),
                Notes = x.Notes
            }).OrderByDescending(x => x.MovementDate).ToList();
        }

        public class MutasiRow
        {
            public string Aset { get; set; } = "";
            public string? UnitCode { get; set; }
            public string Jenis { get; set; } = "";
            public int Quantity { get; set; }
            public DateTime MovementDate { get; set; }
            public string? Referensi { get; set; }
            public string? Notes { get; set; }
        }
    }
}
