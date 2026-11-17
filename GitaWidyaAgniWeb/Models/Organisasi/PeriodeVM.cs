using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Organisasi
{
    /// <summary>
    /// Organisasi › Periode: periode kepengurusan (m_management_period) dan serah terima (t_period_handover + item).
    /// Tab Periode: Draft → Aktif (maks. satu) → Closed (terkunci). Tab Serah Terima: Draft → Diajukan → Disetujui / Ditolak;
    /// persetujuan menutup periode asal, mengaktifkan periode tujuan, dan mengakhiri penugasan periode asal.
    /// </summary>
    public class PeriodeVM
    {
        public class Index
        {
        }

        // ================= Periode
        public class ListPeriode
        {
            public List<PeriodeRow> Rows { get; set; } = Method.GetPeriodeList().ToList();
        }

        public class PeriodeRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public int JumlahPengurus { get; set; }
        }

        public class AddPeriode
        {
            public NewPeriode Row { get; set; } = new();
            public List<GwaVM.OpsiItem> StatusList { get; set; } = GwaVM.Opsi<ManagementPeriodStatus>(ManagementPeriodStatus.Closed);
        }

        public class EditPeriode : AddPeriode
        {
            public EditPeriode(string encKey)
            {
                var obj = LibExtensions.Load<ManagementPeriod>(encKey, "Periode");
                if (obj.Status == ManagementPeriodStatus.Closed)
                {
                    throw new ArgumentException("Periode yang sudah ditutup tidak bisa diubah.");
                }
                Row = new NewPeriode
                {
                    Name = obj.Name,
                    StartDate = Opsi.KeDateTime(obj.StartDate),
                    EndDate = Opsi.KeDateTime(obj.EndDate),
                    Status = (int)obj.Status
                };
            }
        }

        public class NewPeriode
        {
            [Required(ErrorMessage = "Nama periode harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Nama Periode")]
            public string? Name { get; set; }

            [Required(ErrorMessage = "Tanggal mulai harus diisi")]
            [Display(Name = "Tanggal Mulai")]
            public DateTime? StartDate { get; set; }

            [Required(ErrorMessage = "Tanggal selesai harus diisi")]
            [Display(Name = "Tanggal Selesai")]
            public DateTime? EndDate { get; set; }

            [Display(Name = "Status")]
            public int Status { get; set; } = (int)ManagementPeriodStatus.Draft;
        }

        // ================= Serah terima
        public class ListSerahTerima
        {
            public List<SerahTerimaRow> Rows { get; set; } = Method.GetSerahTerimaList().ToList();
        }

        public class SerahTerimaRow
        {
            public string EncDecrKey { get; set; } = "";
            public string? HandoverNumber { get; set; }
            public string FromPeriod { get; set; } = "";
            public string ToPeriod { get; set; } = "";
            public DateTime HandoverDate { get; set; }
            public string? HandedBy { get; set; }
            public string? ReceivedBy { get; set; }
            public string? LpjFileName { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public int JumlahItem { get; set; }
            public bool BisaUbah => Status == (int)HandoverStatus.Draft || Status == (int)HandoverStatus.Ditolak;
        }

        public class AddSerahTerima
        {
            public NewSerahTerima Row { get; set; } = new();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota(null);
            public string? LpjFileName { get; set; }

            public AddSerahTerima()
            {
                Row.FromPeriodId = Opsi.PeriodeAktifId();
                Row.HandoverDate = DateTime.Today;
            }
        }

        public class EditSerahTerima : AddSerahTerima
        {
            public string FromPeriod { get; set; } = "";
            public string StatusText { get; set; } = "";

            public EditSerahTerima(string encKey)
            {
                var obj = LibExtensions.Load<PeriodHandover>(encKey, "Serah terima");
                if (obj.Status != HandoverStatus.Draft && obj.Status != HandoverStatus.Ditolak)
                {
                    throw new ArgumentException("Serah terima hanya bisa diubah saat Draft atau Ditolak.");
                }
                FromPeriod = obj.FromPeriodName;
                StatusText = EnumHelper.Text(obj.Status);
                LpjFileName = obj.LpjFileName;
                Row = new NewSerahTerima
                {
                    FromPeriodId = obj.FromPeriodId,
                    ToPeriodId = obj.ToPeriodId,
                    HandoverNumber = obj.HandoverNumber,
                    HandoverDate = Opsi.KeDateTime(obj.HandoverDate),
                    HandedByMemberId = obj.HandedByMemberId,
                    ReceivedByMemberId = obj.ReceivedByMemberId,
                    Notes = obj.Notes
                };
            }
        }

        public class NewSerahTerima
        {
            [Required(ErrorMessage = "Pilih periode asal")]
            [Display(Name = "Periode Asal")]
            public int? FromPeriodId { get; set; }

            [Required(ErrorMessage = "Pilih periode tujuan")]
            [Display(Name = "Periode Tujuan")]
            public int? ToPeriodId { get; set; }

            [StringLength(100, ErrorMessage = "Nomor maksimal 100 karakter")]
            [Display(Name = "Nomor Berita Acara")]
            public string? HandoverNumber { get; set; }

            [Required(ErrorMessage = "Tanggal serah terima harus diisi")]
            [Display(Name = "Tanggal Serah Terima")]
            public DateTime? HandoverDate { get; set; }

            [Display(Name = "Diserahkan Oleh")]
            public int? HandedByMemberId { get; set; }

            [Display(Name = "Diterima Oleh")]
            public int? ReceivedByMemberId { get; set; }

            [StringLength(1000, ErrorMessage = "Catatan maksimal 1000 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }
        }

        // ---------- Item serah terima (modal _Item + grid _ItemList)
        public class ItemIndex
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public bool BisaUbah { get; set; }
            public NewItem Input { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<HandoverItemType>();
            public List<GwaVM.OpsiItem> KondisiOpsi { get; set; } = GwaVM.Opsi<ItemCondition>();
            public ItemList List { get; set; }

            public ItemIndex(string encKey)
            {
                var obj = LibExtensions.Load<PeriodHandover>(encKey, "Serah terima");
                Key = obj.EncDecrKey;
                Judul = obj.FromPeriodName + " → " + obj.ToPeriodName;
                BisaUbah = obj.Status == HandoverStatus.Draft || obj.Status == HandoverStatus.Ditolak;
                List = new ItemList(obj);
            }
        }

        public class ItemList
        {
            public bool BisaUbah { get; set; }
            public List<ItemRow> Rows { get; set; }

            public ItemList(PeriodHandover obj)
            {
                BisaUbah = obj.Status == HandoverStatus.Draft || obj.Status == HandoverStatus.Ditolak;
                Rows = PeriodHandoverItem.GetDataList(obj.Id).Select(x => new ItemRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Jenis = EnumHelper.Text(x.ItemType),
                    RefId = x.RefId,
                    Description = x.Description,
                    Amount = x.Amount,
                    Quantity = x.Quantity,
                    Kondisi = x.ItemCondition.HasValue ? EnumHelper.Text(x.ItemCondition.Value) : null,
                    Notes = x.Notes
                }).ToList();
            }
            public ItemList(string encKey) : this(LibExtensions.Load<PeriodHandover>(encKey, "Serah terima"))
            {
            }
        }

        public class ItemRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Jenis { get; set; } = "";
            public int? RefId { get; set; }
            public string Description { get; set; } = "";
            public decimal? Amount { get; set; }
            public int? Quantity { get; set; }
            public string? Kondisi { get; set; }
            public string? Notes { get; set; }
        }

        public class NewItem
        {
            public string Key { get; set; } = "";

            [Display(Name = "Jenis")]
            public int ItemType { get; set; } = (int)HandoverItemType.Lainnya;

            [Required(ErrorMessage = "Uraian harus diisi")]
            [StringLength(255, ErrorMessage = "Uraian maksimal 255 karakter")]
            [Display(Name = "Uraian")]
            public string? Description { get; set; }

            [Display(Name = "ID Referensi")]
            public int? RefId { get; set; }

            [Display(Name = "Nominal")]
            public decimal? Amount { get; set; }

            [Display(Name = "Jumlah")]
            public int? Quantity { get; set; }

            [Display(Name = "Kondisi")]
            public int? ItemCondition { get; set; }

            [StringLength(255, ErrorMessage = "Catatan maksimal 255 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }
        }

        public class Method
        {
            public static IEnumerable<PeriodeRow> GetPeriodeList()
            {
                var pengurus = ManagementAssignment.GetDataList().GroupBy(x => x.PeriodId).ToDictionary(g => g.Key, g => g.Count());
                return ManagementPeriod.GetDataList().Select(x => new PeriodeRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Name = x.Name,
                    StartDate = x.StartDate.ToDateTime(TimeOnly.MinValue),
                    EndDate = x.EndDate.ToDateTime(TimeOnly.MinValue),
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    JumlahPengurus = pengurus.TryGetValue(x.Id, out var n) ? n : 0
                }).OrderByDescending(x => x.StartDate);
            }

            public static void SimpanPeriode(NewPeriode input, string user)
            {
                ManagementPeriod.Save(new ManagementPeriod.NewRow
                {
                    Name = input.Name ?? "",
                    StartDate = Opsi.TanggalWajib(input.StartDate, "Tanggal mulai"),
                    EndDate = Opsi.TanggalWajib(input.EndDate, "Tanggal selesai")
                }, user).Ok();
            }

            public static void UpdatePeriode(string encKey, NewPeriode input, string user)
            {
                var obj = LibExtensions.Load<ManagementPeriod>(encKey, "Periode");
                obj.Name = input.Name ?? "";
                obj.StartDate = Opsi.TanggalWajib(input.StartDate, "Tanggal mulai");
                obj.EndDate = Opsi.TanggalWajib(input.EndDate, "Tanggal selesai");
                obj.Status = (ManagementPeriodStatus)input.Status;
                obj.Update(user).Ok();
            }

            public static void HapusPeriode(string? encKey, string user)
            {
                LibExtensions.Load<ManagementPeriod>(encKey, "Periode").Delete(user).Ok();
            }

            public static IEnumerable<SerahTerimaRow> GetSerahTerimaList()
            {
                var nama = Member.GetDataList().ToDictionary(x => x.Id, x => x.Fullname);
                return PeriodHandover.GetDataList().Select(x => new SerahTerimaRow
                {
                    EncDecrKey = x.EncDecrKey,
                    HandoverNumber = x.HandoverNumber,
                    FromPeriod = x.FromPeriodName,
                    ToPeriod = x.ToPeriodName,
                    HandoverDate = x.HandoverDate.ToDateTime(TimeOnly.MinValue),
                    HandedBy = x.HandedByMemberId.HasValue && nama.TryGetValue(x.HandedByMemberId.Value, out var a) ? a : null,
                    ReceivedBy = x.ReceivedByMemberId.HasValue && nama.TryGetValue(x.ReceivedByMemberId.Value, out var b) ? b : null,
                    LpjFileName = x.LpjFileName,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    JumlahItem = x.GetItems(true).Count
                }).OrderByDescending(x => x.HandoverDate);
            }

            public static void SimpanSerahTerima(NewSerahTerima input, byte[]? lpj, string? lpjName, string user)
            {
                var obj = PeriodHandover.Save(new PeriodHandover.NewRow
                {
                    FromPeriodId = input.FromPeriodId ?? 0,
                    ToPeriodId = input.ToPeriodId ?? 0,
                    HandoverNumber = input.HandoverNumber,
                    HandoverDate = Opsi.TanggalWajib(input.HandoverDate, "Tanggal serah terima"),
                    HandedByMemberId = input.HandedByMemberId,
                    ReceivedByMemberId = input.ReceivedByMemberId,
                    Notes = input.Notes
                }, user).Ok();
                if (lpj != null)
                {
                    obj.UpdateLpj(lpj, lpjName, user).Ok();
                }
            }

            public static void UpdateSerahTerima(string encKey, NewSerahTerima input, byte[]? lpj, string? lpjName, bool hapusLpj, string user)
            {
                var obj = LibExtensions.Load<PeriodHandover>(encKey, "Serah terima");
                obj.ToPeriodId = input.ToPeriodId ?? 0;
                obj.HandoverNumber = input.HandoverNumber;
                obj.HandoverDate = Opsi.TanggalWajib(input.HandoverDate, "Tanggal serah terima");
                obj.HandedByMemberId = input.HandedByMemberId;
                obj.ReceivedByMemberId = input.ReceivedByMemberId;
                obj.Notes = input.Notes;
                obj.Update(user).Ok();
                if (lpj != null)
                {
                    obj.UpdateLpj(lpj, lpjName, user).Ok();
                }
                else if (hapusLpj)
                {
                    obj.UpdateLpj(null, null, user).Ok();
                }
            }

            public static PeriodHandover Load(string? encKey) => LibExtensions.Load<PeriodHandover>(encKey, "Serah terima");

            public static void TambahItem(NewItem input, string user)
            {
                var obj = Load(input.Key);
                PeriodHandoverItem.Save(new PeriodHandoverItem.NewRow
                {
                    HandoverId = obj.Id,
                    ItemType = (HandoverItemType)input.ItemType,
                    RefId = input.RefId,
                    Description = input.Description ?? "",
                    Amount = input.Amount,
                    Quantity = input.Quantity,
                    ItemCondition = input.ItemCondition.HasValue ? (ItemCondition)input.ItemCondition.Value : null,
                    Notes = input.Notes
                }, user).Ok();
            }

            public static void HapusItem(string? encKey, string user)
            {
                LibExtensions.Load<PeriodHandoverItem>(encKey, "Item serah terima").Delete(user).Ok();
            }
        }
    }
}
