using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Inventaris;
using GWALib.Lib.Transaksi.Inventaris;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Inventaris
{
    /// <summary>
    /// Inventaris › Aset: tab Aset (m_asset + unit ber-QR + foto + mutasi manual), Kategori, Perawatan (t_maintenance_log),
    /// Inspeksi (t_asset_inspection), Riwayat Mutasi (t_asset_movement). Aset per unit punya kode unit (prefix + 3 digit) untuk QR.
    /// </summary>
    public partial class AsetVM
    {
        public class Index
        {
        }

        public static List<GwaVM.OpsiItem> OpsiAset(bool perUnitSaja = false)
            => Asset.GetDataList().Where(x => x.Status != AssetStatus.Disposed && (!perUnitSaja || x.TrackUnit == UnitTracking.PerUnit))
                .OrderBy(x => x.Name).Select(x => new GwaVM.OpsiItem(x.Id, x.AssetCode + " · " + x.Name)).ToList();

        // ================= Aset
        public class ListAset
        {
            public List<AsetRow> Rows { get; set; }
            public ListAset()
            {
                var foto = AssetPhoto.GetInfoList().GroupBy(x => x.AssetId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.SeqPhoto).First().SeqPhoto);
                Rows = Asset.GetDataList().Select(x => new AsetRow
                {
                    EncDecrKey = x.EncDecrKey,
                    AssetCode = x.AssetCode,
                    Name = x.Name,
                    Brand = x.Brand,
                    Kategori = x.Category?.Name ?? "",
                    Quantity = x.Quantity,
                    Unit = x.Unit,
                    PerUnit = x.TrackUnit == UnitTracking.PerUnit,
                    UnitPrefix = x.UnitPrefix,
                    Location = x.Location,
                    Kondisi = EnumHelper.Text(x.AssetCondition),
                    Sumber = EnumHelper.Text(x.AcquisitionSource),
                    AcquisitionCost = x.AcquisitionCost,
                    RentalFeePerDay = x.RentalFeePerDay,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    FotoSeq = foto.TryGetValue(x.Id, out var s) ? s : null
                }).OrderBy(x => x.Status == (int)AssetStatus.Disposed).ThenBy(x => x.Name).ToList();
            }
        }

        public class AsetRow
        {
            public string EncDecrKey { get; set; } = "";
            public string AssetCode { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Brand { get; set; }
            public string Kategori { get; set; } = "";
            public int Quantity { get; set; }
            public string Unit { get; set; } = "";
            public bool PerUnit { get; set; }
            public string? UnitPrefix { get; set; }
            public string? Location { get; set; }
            public string Kondisi { get; set; } = "";
            public string Sumber { get; set; } = "";
            public decimal? AcquisitionCost { get; set; }
            public decimal RentalFeePerDay { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public int? FotoSeq { get; set; }
            public bool Aktif => Status != (int)AssetStatus.Disposed;
        }

        public class AddAset
        {
            public NewAset Row { get; set; } = new();
            public List<GwaVM.OpsiItem> KategoriOpsi { get; set; } = AssetCategory.GetDataList(FilterAktif.Aktif).OrderBy(x => x.Name).Select(x => new GwaVM.OpsiItem(x.Id, x.Name)).ToList();
            public List<GwaVM.OpsiItem> TrackOpsi { get; set; } = GwaVM.Opsi<UnitTracking>();
            public List<GwaVM.OpsiItem> SumberOpsi { get; set; } = GwaVM.Opsi<AcquisitionSource>();
            public List<GwaVM.OpsiItem> KondisiOpsi { get; set; } = GwaVM.Opsi<ItemCondition>(ItemCondition.Hilang);
            public AddAset() { Row.AcquisitionDate = DateTime.Today; }
        }

        public class EditAset : AddAset
        {
            public string StatusText { get; set; } = "";
            public bool PerUnit { get; set; }
            public EditAset(string encKey)
            {
                var obj = LibExtensions.Load<Asset>(encKey, "Aset");
                StatusText = EnumHelper.Text(obj.Status);
                PerUnit = obj.TrackUnit == UnitTracking.PerUnit;
                Row = new NewAset
                {
                    CategoryId = obj.Category?.Id,
                    AssetCode = obj.AssetCode,
                    Name = obj.Name,
                    Brand = obj.Brand,
                    Specification = obj.Specification,
                    Quantity = obj.Quantity,
                    Unit = obj.Unit,
                    TrackUnit = (int)obj.TrackUnit,
                    UnitPrefix = obj.UnitPrefix,
                    AcquisitionDate = Opsi.KeDateTime(obj.AcquisitionDate),
                    AcquisitionCost = obj.AcquisitionCost,
                    AcquisitionSource = (int)obj.AcquisitionSource,
                    Location = obj.Location,
                    AssetCondition = (int)obj.AssetCondition,
                    RentalFeePerDay = obj.RentalFeePerDay,
                    LateFeePerDay = obj.LateFeePerDay
                };
            }
        }

        public class NewAset
        {
            [Required(ErrorMessage = "Pilih kategori")]
            [Display(Name = "Kategori")]
            public int? CategoryId { get; set; }

            [Required(ErrorMessage = "Kode aset harus diisi")]
            [StringLength(50, ErrorMessage = "Kode maksimal 50 karakter")]
            [Display(Name = "Kode Aset")]
            public string? AssetCode { get; set; }

            [Required(ErrorMessage = "Nama aset harus diisi")]
            [StringLength(150, ErrorMessage = "Nama maksimal 150 karakter")]
            [Display(Name = "Nama Aset")]
            public string? Name { get; set; }

            [StringLength(100, ErrorMessage = "Merek maksimal 100 karakter")]
            [Display(Name = "Merek")]
            public string? Brand { get; set; }

            [Display(Name = "Spesifikasi")]
            public string? Specification { get; set; }

            [Range(0, 100000, ErrorMessage = "Jumlah 0 - 100000")]
            [Display(Name = "Jumlah")]
            public int Quantity { get; set; } = 1;

            [StringLength(30, ErrorMessage = "Satuan maksimal 30 karakter")]
            [Display(Name = "Satuan")]
            public string? Unit { get; set; } = "unit";

            [Display(Name = "Pelacakan")]
            public int TrackUnit { get; set; } = (int)UnitTracking.CukupJumlah;

            [StringLength(10, ErrorMessage = "Prefix maksimal 10 karakter")]
            [Display(Name = "Prefix Kode Unit (mis. HT)")]
            public string? UnitPrefix { get; set; }

            [Display(Name = "Tanggal Perolehan")]
            public DateTime? AcquisitionDate { get; set; }

            [Display(Name = "Nilai Perolehan (Rp)")]
            public decimal? AcquisitionCost { get; set; }

            [Display(Name = "Sumber")]
            public int AcquisitionSource { get; set; } = (int)GWAShared.Enums.AcquisitionSource.Beli;

            [StringLength(150, ErrorMessage = "Lokasi maksimal 150 karakter")]
            [Display(Name = "Lokasi Simpan")]
            public string? Location { get; set; }

            [Display(Name = "Kondisi")]
            public int AssetCondition { get; set; } = (int)ItemCondition.Baik;

            [Range(0, 999999999, ErrorMessage = "Tarif tidak boleh negatif")]
            [Display(Name = "Tarif Sewa / Hari")]
            public decimal RentalFeePerDay { get; set; }

            [Range(0, 999999999, ErrorMessage = "Denda tidak boleh negatif")]
            [Display(Name = "Denda Telat / Hari")]
            public decimal LateFeePerDay { get; set; }
        }

        // Unit ber-QR (modal _Unit).
        public class Unit
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public UnitInput Input { get; set; } = new();
            public List<GwaVM.OpsiItem> KondisiOpsi { get; set; } = GwaVM.Opsi<ItemCondition>(ItemCondition.Hilang);
            public List<UnitRow> Rows { get; set; }

            public Unit(string encKey)
            {
                var obj = LibExtensions.Load<Asset>(encKey, "Aset");
                if (obj.TrackUnit != UnitTracking.PerUnit)
                {
                    throw new ArgumentException("Aset ini dilacak per jumlah, tidak punya unit.");
                }
                Key = obj.EncDecrKey;
                Nama = obj.AssetCode + " · " + obj.Name;
                Rows = obj.GetUnits(true).Select(x => new UnitRow
                {
                    EncDecrKey = x.EncDecrKey,
                    UnitCode = x.UnitCode,
                    SerialNumber = x.SerialNumber,
                    Kondisi = EnumHelper.Text(x.UnitCondition),
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    LabelPrintedAt = x.LabelPrintedAt,
                    Notes = x.Notes
                }).OrderBy(x => x.UnitCode).ToList();
            }
        }

        public class UnitRow
        {
            public string EncDecrKey { get; set; } = "";
            public string UnitCode { get; set; } = "";
            public string? SerialNumber { get; set; }
            public string Kondisi { get; set; } = "";
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public DateTime? LabelPrintedAt { get; set; }
            public string? Notes { get; set; }
        }

        public class UnitInput
        {
            public string Key { get; set; } = "";

            [StringLength(100, ErrorMessage = "Nomor seri maksimal 100 karakter")]
            [Display(Name = "Nomor Seri")]
            public string? SerialNumber { get; set; }

            [Display(Name = "Kondisi")]
            public int UnitCondition { get; set; } = (int)ItemCondition.Baik;

            [StringLength(255, ErrorMessage = "Catatan maksimal 255 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }
        }

        public class LabelInput
        {
            // encKey aset pemilik unit.
            public string? AsetKey { get; set; }
            public List<string> Keys { get; set; } = new();
        }

        // Foto aset (modal _Foto).
        public class Foto
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public List<AssetPhoto.Info> Items { get; set; }
            public Foto(string encKey)
            {
                var obj = LibExtensions.Load<Asset>(encKey, "Aset");
                Key = obj.EncDecrKey;
                Nama = obj.Name;
                Items = obj.GetPhotos(true).OrderBy(x => x.SeqPhoto).ToList();
            }
        }

        public class FotoInput
        {
            public string Key { get; set; } = "";
            public int Seq { get; set; }
        }

        // Mutasi manual (aset per jumlah).
        public class Mutasi
        {
            public string Nama { get; set; } = "";
            public MutasiInput Row { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = new()
            {
                new((int)AssetMovementType.In, "Masuk"), new((int)AssetMovementType.Out, "Keluar"), new((int)AssetMovementType.Adjustment, "Penyesuaian (+/-)")
            };
            public Mutasi(string encKey)
            {
                var obj = LibExtensions.Load<Asset>(encKey, "Aset");
                if (obj.TrackUnit == UnitTracking.PerUnit)
                {
                    throw new ArgumentException("Aset per unit: tambah / hapus lewat daftar unit.");
                }
                Nama = obj.AssetCode + " · " + obj.Name + " (stok " + obj.Quantity + " " + obj.Unit + ")";
                Row.Key = obj.EncDecrKey;
            }
        }

        public class MutasiInput
        {
            public string Key { get; set; } = "";

            [Display(Name = "Jenis")]
            public int MovementType { get; set; } = (int)AssetMovementType.In;

            [Range(-100000, 100000, ErrorMessage = "Jumlah tidak valid")]
            [Display(Name = "Jumlah")]
            public int Quantity { get; set; } = 1;

            [StringLength(255, ErrorMessage = "Catatan maksimal 255 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }
        }
    }
}
