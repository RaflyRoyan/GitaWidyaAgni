using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Mitra;
using GWALib.Lib.Master.Sponsorship;
using GWALib.Lib.Transaksi.Keuangan;
using GWALib.Lib.Transaksi.Sponsorship;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Kemitraan
{
    /// <summary>
    /// Kemitraan › Sponsorship: tab Sponsorship (t_sponsorship + benefit + MoU + penerimaan dana) dan tab Paket (m_sponsorship_package + benefit).
    /// Aktif → Selesai (semua benefit diberikan) / Batal (belum ada dana masuk).
    /// </summary>
    public partial class SponsorshipVM
    {
        public class Index
        {
        }

        // ================= Sponsorship
        public class ListSponsor
        {
            public List<SponsorRow> Rows { get; set; } = SponsorshipDeal.GetDataList().Select(x => new SponsorRow
            {
                EncDecrKey = x.EncDecrKey,
                Nomor = x.SponsorshipNumber,
                Mitra = x.ExternalPartyName,
                Paket = x.PackageName,
                Bentuk = EnumHelper.Text(x.SponsorshipForm),
                Dana = x.SponsorshipForm == SponsorshipForm.Dana,
                Amount = x.Amount,
                ReceivedAmount = x.ReceivedAmount,
                InKind = x.InKindDescription,
                StartDate = Opsi.KeDateTime(x.StartDate),
                EndDate = Opsi.KeDateTime(x.EndDate),
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status)
            }).OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Nomor).ToList();
        }

        public class SponsorRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Nomor { get; set; } = "";
            public string Mitra { get; set; } = "";
            public string? Paket { get; set; }
            public string Bentuk { get; set; } = "";
            public bool Dana { get; set; }
            public decimal Amount { get; set; }
            public decimal ReceivedAmount { get; set; }
            public string? InKind { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
        }

        public class AddSponsor
        {
            public NewSponsor Row { get; set; } = new() { StartDate = DateTime.Today };
            public List<GwaVM.OpsiItem> MitraOpsi { get; set; } = Opsi.Mitra();
            public List<GwaVM.OpsiItem> PaketOpsi { get; set; } = SponsorshipPackage.GetDataList(FilterAktif.Aktif).OrderBy(x => x.SeqNo)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Name + " · Rp " + x.Price.ToString("#,##0"))).ToList();
            public List<GwaVM.OpsiItem> BentukOpsi { get; set; } = GwaVM.Opsi<SponsorshipForm>();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan();
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<GwaVM.OpsiItem> KontakOpsi { get; set; } = new();
            public AddSponsor() { Row.PeriodId = Opsi.PeriodeAktifId(); }
        }

        public class EditSponsor : AddSponsor
        {
            public string Nomor { get; set; } = "";
            public EditSponsor(string encKey)
            {
                var obj = LibExtensions.Load<SponsorshipDeal>(encKey, "Sponsorship");
                if (obj.Status != SponsorshipStatus.Aktif)
                {
                    throw new ArgumentException("Sponsorship hanya bisa diubah saat Aktif.");
                }
                Nomor = obj.SponsorshipNumber + " · " + obj.ExternalPartyName + (obj.PackageName != null ? " · paket " + obj.PackageName : "");
                KontakOpsi = ExternalPartyContact.GetDataList(FilterAktif.Aktif, obj.ExternalPartyId).Select(c => new GwaVM.OpsiItem(c.Id, c.Name)).ToList();
                Row = new NewSponsor
                {
                    ExternalPartyId = obj.ExternalPartyId,
                    ContactId = obj.ContactId,
                    PeriodId = obj.PeriodId,
                    ActivityId = obj.ActivityId,
                    SponsorshipForm = (int)obj.SponsorshipForm,
                    Amount = obj.Amount,
                    InKindDescription = obj.InKindDescription,
                    StartDate = Opsi.KeDateTime(obj.StartDate),
                    EndDate = Opsi.KeDateTime(obj.EndDate),
                    PicMemberId = obj.PicMemberId,
                    Notes = obj.Notes
                };
            }
        }

        public class NewSponsor
        {
            [Required(ErrorMessage = "Pilih mitra")]
            [Display(Name = "Mitra / Sponsor")]
            public int? ExternalPartyId { get; set; }

            [Display(Name = "Kontak")]
            public int? ContactId { get; set; }

            [Display(Name = "Paket (tetap setelah dibuat)")]
            public int? PackageId { get; set; }

            [Display(Name = "Bentuk")]
            public int SponsorshipForm { get; set; } = (int)GWAShared.Enums.SponsorshipForm.Dana;

            [Display(Name = "Nominal (0 = harga paket)")]
            public decimal Amount { get; set; }

            [StringLength(500, ErrorMessage = "Uraian maksimal 500 karakter")]
            [Display(Name = "Uraian Barang / Jasa / Media")]
            public string? InKindDescription { get; set; }

            [Display(Name = "Mulai")]
            public DateTime? StartDate { get; set; }

            [Display(Name = "Selesai")]
            public DateTime? EndDate { get; set; }

            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [Display(Name = "Kegiatan")]
            public int? ActivityId { get; set; }

            [Display(Name = "PIC")]
            public int? PicMemberId { get; set; }

            [StringLength(500, ErrorMessage = "Catatan maksimal 500 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }
        }

        // Detail: benefit, MoU, dana masuk.
        public class Detail
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public string Ringkasan { get; set; } = "";
            public bool Aktif { get; set; }
            public bool BisaMou { get; set; }
            public List<BenefitRow> Benefit { get; set; }
            public List<MouRow> Mou { get; set; }
            public List<DanaRow> Dana { get; set; }

            public Detail(string encKey)
            {
                var obj = LibExtensions.Load<SponsorshipDeal>(encKey, "Sponsorship");
                Key = obj.EncDecrKey;
                Aktif = obj.Status == SponsorshipStatus.Aktif;
                BisaMou = obj.Status != SponsorshipStatus.Batal;
                Judul = obj.SponsorshipNumber + " · " + obj.ExternalPartyName;
                Ringkasan = EnumHelper.Text(obj.SponsorshipForm) + (obj.SponsorshipForm == SponsorshipForm.Dana
                    ? $" Rp {obj.Amount:#,##0} · diterima Rp {obj.ReceivedAmount:#,##0}" : " · " + obj.InKindDescription) + " · " + EnumHelper.Text(obj.Status);
                Benefit = obj.GetBenefits(true).Select(x => new BenefitRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Title = x.Title,
                    Description = x.Description,
                    Sudah = x.IsDone == SponsorshipBenefitStatus.Sudah,
                    DoneAt = x.DoneAt,
                    ProofFileName = x.ProofFileName,
                    Notes = x.Notes
                }).ToList();
                Mou = obj.GetMous(true).Select(x => new MouRow
                {
                    EncDecrKey = x.EncDecrKey,
                    MouNumber = x.MouNumber,
                    SignedDate = Opsi.KeDateTime(x.SignedDate),
                    Masa = (x.StartDate?.ToString("dd MMM yyyy") ?? "?") + " – " + (x.EndDate?.ToString("dd MMM yyyy") ?? "?"),
                    DocumentFileName = x.DocumentFileName
                }).ToList();
                Dana = FinancialTransaction.GetDataList(transactionType: CashFlowType.Income, status: TransactionStatus.Posted)
                    .Where(t => t.SponsorshipId == obj.Id)
                    .Select(t => new DanaRow { Nomor = t.TransactionNumber, Tanggal = t.TransactionDate.ToDateTime(TimeOnly.MinValue), Akun = t.AccountName, Amount = t.Amount })
                    .ToList();
            }
        }

        public class BenefitRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string? Description { get; set; }
            public bool Sudah { get; set; }
            public DateTime? DoneAt { get; set; }
            public string? ProofFileName { get; set; }
            public string? Notes { get; set; }
        }

        public class MouRow
        {
            public string EncDecrKey { get; set; } = "";
            public string MouNumber { get; set; } = "";
            public DateTime? SignedDate { get; set; }
            public string Masa { get; set; } = "";
            public string? DocumentFileName { get; set; }
        }

        public class DanaRow
        {
            public string Nomor { get; set; } = "";
            public DateTime Tanggal { get; set; }
            public string Akun { get; set; } = "";
            public decimal Amount { get; set; }
        }

        public class BenefitInput
        {
            public string Key { get; set; } = "";
            public string? Title { get; set; }
            public string? Description { get; set; }
        }

        public class MouInput
        {
            public string Key { get; set; } = "";
            public string? MouNumber { get; set; }
            public DateTime? SignedDate { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? Notes { get; set; }
        }

        // ================= Paket
        public class ListPaket
        {
            public List<PaketRow> Rows { get; set; } = SponsorshipPackage.GetDataList(FilterAktif.Semua).Select(x => new PaketRow
            {
                EncDecrKey = x.EncDecrKey,
                Name = x.Name,
                Price = x.Price,
                SeqNo = x.SeqNo,
                Benefit = string.Join(", ", x.GetBenefits(true).OrderBy(b => b.SeqNo).Select(b => b.Title)),
                Aktif = x.IsActive == ActiveStatus.Aktif
            }).OrderBy(x => x.SeqNo).ToList();
        }

        public class PaketRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public decimal Price { get; set; }
            public int SeqNo { get; set; }
            public string Benefit { get; set; } = "";
            public bool Aktif { get; set; }
        }

        public class AddPaket
        {
            public NewPaket Row { get; set; } = new();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
            public List<PackageBenefit> Benefit { get; set; } = new();
            public string? Key { get; set; }
        }

        public class EditPaket : AddPaket
        {
            public EditPaket(string encKey)
            {
                var obj = LibExtensions.Load<SponsorshipPackage>(encKey, "Paket");
                Key = obj.EncDecrKey;
                Benefit = obj.GetBenefits(true).OrderBy(b => b.SeqNo).ToList();
                Row = new NewPaket { Name = obj.Name, Price = obj.Price, SeqNo = obj.SeqNo, IsActive = (int)obj.IsActive };
            }
        }

        public class NewPaket
        {
            [Required(ErrorMessage = "Nama paket harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Nama Paket")]
            public string? Name { get; set; }

            [Range(0, 999999999999, ErrorMessage = "Harga tidak boleh negatif")]
            [Display(Name = "Harga")]
            public decimal Price { get; set; }

            [Range(0, 999, ErrorMessage = "Urutan 0 - 999")]
            [Display(Name = "Urutan (0 = paling akhir)")]
            public int SeqNo { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }
    }
}
