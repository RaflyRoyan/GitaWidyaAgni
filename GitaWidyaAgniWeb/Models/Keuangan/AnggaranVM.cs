using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Keuangan;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Keuangan
{
    /// <summary>
    /// Keuangan › Anggaran (t_budget + item + dokumen). Draft → Diajukan → Disetujui / Ditolak; Ditolak bisa diubah dan diajukan lagi.
    /// Item dan dokumen hanya berubah saat Draft / Ditolak. Realisasi item = pengeluaran Diposting yang memakai mata anggaran itu.
    /// </summary>
    public class AnggaranVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Budget.GetDataList().Select(x => new Row
            {
                EncDecrKey = x.EncDecrKey,
                Name = x.Name,
                Periode = x.PeriodName,
                TotalAmount = x.TotalAmount,
                Realisasi = x.GetItems(true).Sum(i => i.RealizedAmount),
                JumlahItem = x.GetItems(false).Count,
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status),
                ApprovedAt = x.ApprovedAt
            }).OrderByDescending(x => x.Periode).ThenBy(x => x.Name).ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string Periode { get; set; } = "";
            public decimal TotalAmount { get; set; }
            public decimal Realisasi { get; set; }
            public int Persen => TotalAmount == 0 ? 0 : (int)Math.Round(Realisasi * 100 / TotalAmount);
            public int JumlahItem { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public DateTime? ApprovedAt { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public Add() { Row.PeriodId = Opsi.PeriodeAktifId(); }
        }

        public class Edit : Add
        {
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<Budget>(encKey, "Anggaran");
                Row = new NewRow { PeriodId = obj.PeriodId, Name = obj.Name };
            }
        }

        public class NewRow
        {
            [Required(ErrorMessage = "Pilih periode")]
            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [Required(ErrorMessage = "Nama anggaran harus diisi")]
            [StringLength(150, ErrorMessage = "Nama maksimal 150 karakter")]
            [Display(Name = "Nama Anggaran")]
            public string? Name { get; set; }
        }

        // ---------- Rincian (modal _Rincian): item + dokumen
        public class Rincian
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public string StatusText { get; set; } = "";
            public bool BisaUbah { get; set; }
            public ItemInput Input { get; set; } = new();
            public List<GwaVM.OpsiItem> KategoriOpsi { get; set; } = Opsi.Kategori(CashFlowType.Expense);
            public List<GwaVM.OpsiItem> ProgramOpsi { get; set; }
            public ItemList List { get; set; }
            public List<DokumenRow> Dokumen { get; set; }

            public Rincian(string encKey)
            {
                var obj = LibExtensions.Load<Budget>(encKey, "Anggaran");
                Key = obj.EncDecrKey;
                Nama = obj.Name + " · " + obj.PeriodName;
                StatusText = EnumHelper.Text(obj.Status);
                BisaUbah = obj.Status == BudgetStatus.Draft || obj.Status == BudgetStatus.Rejected;
                ProgramOpsi = Opsi.Program(obj.PeriodId);
                List = new ItemList(obj);
                Dokumen = obj.GetDocuments(true).Select(x => new DokumenRow { EncDecrKey = x.EncDecrKey, FileName = x.FileName }).ToList();
            }
        }

        public class ItemList
        {
            public bool BisaUbah { get; set; }
            public List<ItemRow> Rows { get; set; }

            public ItemList(Budget obj)
            {
                BisaUbah = obj.Status == BudgetStatus.Draft || obj.Status == BudgetStatus.Rejected;
                var program = WorkProgramNames();
                Rows = obj.GetItems(true).Select(x => new ItemRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Name = x.Name,
                    Kategori = x.CategoryName,
                    Program = x.ProgramId.HasValue && program.TryGetValue(x.ProgramId.Value, out var p) ? p : null,
                    Quantity = x.Quantity,
                    Unit = x.Unit,
                    UnitPrice = x.UnitPrice,
                    Amount = x.Amount,
                    Realisasi = x.RealizedAmount
                }).ToList();
            }
            public ItemList(string encKey) : this(LibExtensions.Load<Budget>(encKey, "Anggaran"))
            {
            }

            private static Dictionary<int, string> WorkProgramNames()
                => GWALib.Lib.Transaksi.Program.WorkProgram.GetDataList().ToDictionary(x => x.Id, x => x.Title);
        }

        public class ItemRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string Kategori { get; set; } = "";
            public string? Program { get; set; }
            public decimal Quantity { get; set; }
            public string? Unit { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Amount { get; set; }
            public decimal Realisasi { get; set; }
            public decimal Sisa => Amount - Realisasi;
        }

        public class ItemInput
        {
            public string Key { get; set; } = "";

            [Required(ErrorMessage = "Uraian harus diisi")]
            [StringLength(200, ErrorMessage = "Uraian maksimal 200 karakter")]
            [Display(Name = "Uraian")]
            public string? Name { get; set; }

            [Required(ErrorMessage = "Pilih kategori")]
            [Display(Name = "Kategori")]
            public int? CategoryId { get; set; }

            [Display(Name = "Program")]
            public int? ProgramId { get; set; }

            [Range(0.01, 999999999, ErrorMessage = "Jumlah harus lebih dari 0")]
            [Display(Name = "Jumlah")]
            public decimal Quantity { get; set; } = 1;

            [StringLength(30, ErrorMessage = "Satuan maksimal 30 karakter")]
            [Display(Name = "Satuan")]
            public string? Unit { get; set; }

            [Range(0, 999999999999, ErrorMessage = "Harga tidak boleh negatif")]
            [Display(Name = "Harga Satuan")]
            public decimal UnitPrice { get; set; }
        }

        public class DokumenRow
        {
            public string EncDecrKey { get; set; } = "";
            public string FileName { get; set; } = "";
        }

        public class Method
        {
            public static Budget Load(string? key) => LibExtensions.Load<Budget>(key, "Anggaran");

            public static void Simpan(NewRow input, string user)
                => Budget.Save(new Budget.NewRow { PeriodId = input.PeriodId ?? 0, Name = input.Name ?? "" }, user).Ok();

            public static void Update(string key, NewRow input, string user)
            {
                var obj = Load(key);
                obj.Name = input.Name ?? "";
                obj.Update(user).Ok();
            }

            public static void TambahItem(ItemInput input, string user)
            {
                var obj = Load(input.Key);
                BudgetItem.Save(new BudgetItem.NewRow
                {
                    BudgetId = obj.Id,
                    Name = input.Name ?? "",
                    CategoryId = input.CategoryId ?? 0,
                    ProgramId = input.ProgramId,
                    Quantity = input.Quantity,
                    Unit = input.Unit,
                    UnitPrice = input.UnitPrice
                }, user).Ok();
            }

            public static void HapusItem(string? key, string user) => LibExtensions.Load<BudgetItem>(key, "Item anggaran").Delete(user).Ok();

            public static void TambahDokumen(string? key, byte[]? data, IFormFile? meta, string user)
            {
                var obj = Load(key);
                if (data == null)
                {
                    throw new ArgumentException("Pilih berkas terlebih dahulu.");
                }
                BudgetDocument.Save(new BudgetDocument.NewRow { BudgetId = obj.Id, FileData = data, FileName = meta!.FileName, FileType = meta.ContentType }, user).Ok();
            }

            public static BudgetDocument LoadDokumen(string? key) => LibExtensions.Load<BudgetDocument>(key, "Dokumen anggaran");
        }
    }
}
