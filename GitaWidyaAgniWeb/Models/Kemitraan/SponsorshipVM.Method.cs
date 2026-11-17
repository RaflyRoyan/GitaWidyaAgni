using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Mitra;
using GWALib.Lib.Master.Sponsorship;
using GWALib.Lib.Transaksi.Sponsorship;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Kemitraan
{
    public partial class SponsorshipVM
    {
        public class Method
        {
            public static SponsorshipDeal Load(string? key) => LibExtensions.Load<SponsorshipDeal>(key, "Sponsorship");

            public static List<GwaVM.OpsiItem> OpsiKontak(int partyId)
                => ExternalPartyContact.GetDataList(FilterAktif.Aktif, partyId).Select(c => new GwaVM.OpsiItem(c.Id, c.Name + (c.IsPrimary == ContactRank.Utama ? " (utama)" : ""))).ToList();

            public static void Simpan(NewSponsor input, string user)
                => SponsorshipDeal.Save(new SponsorshipDeal.NewRow
                {
                    ExternalPartyId = input.ExternalPartyId ?? 0,
                    ContactId = input.ContactId,
                    PackageId = input.PackageId,
                    PeriodId = input.PeriodId,
                    ActivityId = input.ActivityId,
                    SponsorshipForm = (SponsorshipForm)input.SponsorshipForm,
                    Amount = input.Amount,
                    InKindDescription = input.InKindDescription,
                    StartDate = Opsi.Tanggal(input.StartDate),
                    EndDate = Opsi.Tanggal(input.EndDate),
                    PicMemberId = input.PicMemberId,
                    Notes = input.Notes
                }, user).Ok();

            public static void Update(string key, NewSponsor input, string user)
            {
                var obj = Load(key);
                obj.ExternalPartyId = input.ExternalPartyId ?? obj.ExternalPartyId;
                obj.ContactId = input.ContactId;
                obj.PeriodId = input.PeriodId;
                obj.ActivityId = input.ActivityId;
                obj.SponsorshipForm = (SponsorshipForm)input.SponsorshipForm;
                obj.Amount = input.Amount;
                obj.InKindDescription = input.InKindDescription;
                obj.StartDate = Opsi.Tanggal(input.StartDate);
                obj.EndDate = Opsi.Tanggal(input.EndDate);
                obj.PicMemberId = input.PicMemberId;
                obj.Notes = input.Notes;
                obj.Update(user).Ok();
            }

            public static GwaVM.BayarForm FormTerima(string encKey)
            {
                var obj = Load(encKey);
                return new GwaVM.BayarForm
                {
                    Judul = obj.SponsorshipNumber + " · " + obj.ExternalPartyName,
                    Info = $"Sisa Rp {Math.Max(0, obj.Amount - obj.ReceivedAmount):#,##0}. Dicatat sebagai transaksi Pemasukan.",
                    OnSave = "Sponsorship.receive()",
                    Tombol = "Catat Dana Masuk",
                    KategoriOpsi = Opsi.Kategori(CashFlowType.Income),
                    Row = new GwaVM.BayarInput { Key = obj.EncDecrKey, Amount = Math.Max(0, obj.Amount - obj.ReceivedAmount) }
                };
            }

            public static void Terima(GwaVM.BayarInput input, string user)
                => Load(input.Key).ReceivePayment(input.AccountId ?? 0, input.CategoryId ?? 0, input.Amount ?? 0, Opsi.TanggalWajib(input.Date, "Tanggal"), user).Ok();

            // ---------- Benefit
            public static void TambahBenefit(BenefitInput input, string user)
            {
                var obj = Load(input.Key);
                SponsorshipBenefit.Save(new SponsorshipBenefit.NewRow { SponsorshipId = obj.Id, Title = input.Title ?? "", Description = input.Description }, user).Ok();
            }

            public static SponsorshipBenefit LoadBenefit(string? key) => LibExtensions.Load<SponsorshipBenefit>(key, "Benefit");

            public static void Selesai(string? key, byte[]? bukti, IFormFile? meta, string? notes, string user)
                => LoadBenefit(key).MarkDone(bukti, meta?.FileName, meta?.ContentType, notes, user).Ok();

            // ---------- MoU
            public static void TambahMou(MouInput input, byte[]? doc, IFormFile? meta, string user)
            {
                var obj = Load(input.Key);
                SponsorshipMou.Save(new SponsorshipMou.NewRow
                {
                    SponsorshipId = obj.Id,
                    MouNumber = input.MouNumber,
                    SignedDate = Opsi.Tanggal(input.SignedDate),
                    StartDate = Opsi.Tanggal(input.StartDate),
                    EndDate = Opsi.Tanggal(input.EndDate),
                    Notes = input.Notes,
                    Document = doc,
                    DocumentFileName = meta?.FileName,
                    DocumentContentType = meta?.ContentType
                }, user).Ok();
            }

            public static SponsorshipMou LoadMou(string? key) => LibExtensions.Load<SponsorshipMou>(key, "MoU");

            // ---------- Paket
            public static void SimpanPaket(NewPaket input, string user)
                => SponsorshipPackage.Save(new SponsorshipPackage.NewRow { Name = input.Name ?? "", Price = input.Price, SeqNo = input.SeqNo }, user).Ok();

            public static void UpdatePaket(string key, NewPaket input, string user)
            {
                var obj = LibExtensions.Load<SponsorshipPackage>(key, "Paket");
                obj.Name = input.Name ?? "";
                obj.Price = input.Price;
                obj.SeqNo = input.SeqNo;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }

            public static void HapusPaket(string? key, string user) => LibExtensions.Load<SponsorshipPackage>(key, "Paket").Delete(user).Ok();

            public static void TambahBenefitPaket(BenefitInput input, string user)
            {
                var obj = LibExtensions.Load<SponsorshipPackage>(input.Key, "Paket");
                PackageBenefit.Save(new PackageBenefit.NewRow { PackageId = obj.Id, Title = input.Title ?? "", Description = input.Description }, user).Ok();
            }

            public static void HapusBenefitPaket(string? key, string user) => LibExtensions.Load<PackageBenefit>(key, "Benefit paket").Delete(user).Ok();
        }
    }
}
