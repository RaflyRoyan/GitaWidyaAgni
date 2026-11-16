using GWALib.Lib.Master.Merchandise;
using GWALib.Lib.Master.Mitra;
using GWALib.Lib.Master.Inventaris;
using GWALib.Lib.Transaksi.Keuangan;
using GWALib.Lib.Transaksi.Merchandise;
using GWALib.Lib.Transaksi.Pengadaan;
using GWALib.Lib.Transaksi.Sponsorship;
using GWAShared.Enums;

namespace GWASeeder
{
    internal static partial class Seed
    {
        public static int BudgetItemConcertId;
        public static FundingRequest FundingForProcurement = null!;
        public static SponsorshipDeal SponsorBni = null!;

        private static string BendaharaUser => UserOf(Bendahara);
        private static string KetuaUser => UserOf(Ketua);

        // ---------------------------------------------------------------- Keuangan
        private static void SeedFinance()
        {
            Ok(PeriodBalance.Open(PeriodActive.Id, null, Admin), "Buka saldo periode");

            FinancialTransaction Trx(string account, string category, CashFlowType type, decimal amount, int daysAgo, string description, bool post = true, int? activityId = null, int? programId = null)
            {
                var trx = Ok(FinancialTransaction.Save(new FinancialTransaction.NewRow
                {
                    AccountId = Accounts[account], CategoryId = Categories[category], PeriodId = PeriodActive.Id, TransactionType = type, Amount = amount,
                    TransactionDate = Today.AddDays(-daysAgo), Description = description, ActivityId = activityId, ProgramId = programId
                }, Admin), "Transaksi " + description);
                return post ? Ok(trx.Post(Admin), "Posting " + description) : trx;
            }
            Trx("Bank", "Hibah Kampus", CashFlowType.Income, 10_000_000, 70, "Dana hibah kemahasiswaan semester ganjil");
            Trx("Kas", "Donasi", CashFlowType.Income, 1_500_000, 60, "Donasi alumni angkatan 2015");
            var consumption = Trx("Kas", "Konsumsi", CashFlowType.Expense, 850_000, 42, "Konsumsi penampilan Dies Natalis", activityId: Activities["DiesNatalis"].Id);
            Ok(FinancialAttachment.Save(new FinancialAttachment.NewRow { TransactionId = consumption.Id, FileData = Photo(9), FileName = "nota-konsumsi.jpg", FileType = "image/jpeg" }, Admin), "Lampiran nota");
            Trx("Bank", "Pendaftaran Lomba", CashFlowType.Expense, 1_250_000, 35, "Biaya pendaftaran Kejuaraan MB Jatim", activityId: Activities["LombaJatim"].Id);
            Trx("Bank", "Transportasi", CashFlowType.Expense, 2_400_000, 24, "Sewa bus rombongan Kejuaraan Jatim", activityId: Activities["LombaJatim"].Id);
            Trx("Kas", "Honor Pelatih", CashFlowType.Expense, 600_000, 10, "Honor pelatih brass bulan Oktober", programId: Programs["Latihan"].Id);
            Trx("Kas", "Administrasi", CashFlowType.Expense, 75_000, 3, "Fotokopi proposal konser", post: false);

            var budget = Ok(Budget.Save(new Budget.NewRow
            {
                PeriodId = PeriodActive.Id, Name = "Anggaran Kegiatan 2026/2027",
                Items = new()
                {
                    new() { CategoryId = Categories["Perlengkapan & Alat"], ProgramId = Programs["Konser"].Id, Name = "Sewa sound system & lighting konser", Quantity = 1, Unit = "paket", UnitPrice = 7_500_000 },
                    new() { CategoryId = Categories["Konsumsi"], ProgramId = Programs["Konser"].Id, Name = "Konsumsi pemain & panitia konser", Quantity = 80, Unit = "porsi", UnitPrice = 25_000 },
                    new() { CategoryId = Categories["Pendaftaran Lomba"], ProgramId = Programs["Kejurnas"].Id, Name = "Pendaftaran GPMB 2027", Quantity = 1, Unit = "tim", UnitPrice = 3_500_000 },
                    new() { CategoryId = Categories["Perlengkapan & Alat"], ProgramId = Programs["Kejurnas"].Id, Name = "Pembelian alat perkusi tambahan", Quantity = 2, Unit = "unit", UnitPrice = 7_500_000 },
                    new() { CategoryId = Categories["Kostum"], ProgramId = Programs["Kejurnas"].Id, Name = "Kostum lomba", Quantity = 45, Unit = "set", UnitPrice = 350_000 },
                    new() { CategoryId = Categories["Honor Pelatih"], ProgramId = Programs["Latihan"].Id, Name = "Honor pelatih", Quantity = 10, Unit = "bulan", UnitPrice = 600_000 }
                }
            }, Admin), "Anggaran");
            Ok(BudgetDocument.Save(new BudgetDocument.NewRow { BudgetId = budget.Id, FileData = Pdf("RAB Kegiatan 2026/2027"), FileName = "rab-2026-2027.pdf", FileType = "application/pdf" }, Admin), "Dokumen anggaran");
            Ok(budget.Submit(Admin), "Ajukan anggaran");
            Ok(budget.Approve(KetuaUser), "Setujui anggaran");
            var budgetItems = BudgetItem.GetDataList(budget.Id);
            BudgetItemConcertId = budgetItems.First(x => x.Name.StartsWith("Sewa sound")).Id;
            var budgetPercussion = budgetItems.First(x => x.Name.StartsWith("Pembelian alat")).Id;
            var draftBudget = Ok(Budget.Save(new Budget.NewRow
            {
                PeriodId = PeriodActive.Id, Name = "Anggaran Tambahan Diklat",
                Items = new() { new() { CategoryId = Categories["Konsumsi"], ProgramId = Programs["Rekrutmen"].Id, Name = "Konsumsi diklat", Quantity = 50, Unit = "porsi", UnitPrice = 20_000 } }
            }, Admin), "Anggaran draft");
            Ok(draftBudget.Submit(Admin), "Ajukan anggaran tambahan");
            Ok(draftBudget.Reject(KetuaUser), "Tolak anggaran tambahan");

            // Iuran November: dibuat untuk semua anggota aktif, sebagian dibayar, satu dibebaskan.
            Ok(DuesBill.Generate(Dues.Id, "2026-11", Today.AddDays(14), null, Admin), "Tagihan iuran");
            Ok(DuesBill.Generate(Dues.Id, "2026-10", Today.AddDays(-16), Members.Take(12).Select(m => m.Id).ToList(), Admin), "Tagihan iuran Oktober");
            var bills = DuesBill.GetDataList(Dues.Id, null, "2026-10");
            for (var i = 0; i < bills.Count; i++)
            {
                if (i == bills.Count - 1)
                {
                    Ok(bills[i].Waive(Admin), "Bebaskan iuran");
                }
                else if (i % 4 != 3)
                {
                    Ok(bills[i].Pay(i % 2 == 0 ? Accounts["Kas"] : Accounts["EWallet"], Today.AddDays(-20 + i), null, Admin), "Bayar iuran");
                }
            }
            foreach (var bill in DuesBill.GetDataList(Dues.Id, null, "2026-11").Take(5))
            {
                Ok(bill.Pay(Accounts["EWallet"], Today.AddDays(-1), null, Admin), "Bayar iuran November");
            }

            // Pengajuan dana konser: disetujui 2 langkah, dicairkan, direalisasi, diselesaikan.
            var funding = Ok(FundingRequest.Save(new FundingRequest.NewRow
            {
                BudgetItemId = BudgetItemConcertId, ActivityId = Activities["Konser"].Id, Purpose = "Uang muka sewa sound system konser", Amount = 5_000_000
            }, UserOf(Members[11])), "Pengajuan dana konser");
            funding = Ok(funding.Submit(UserOf(Members[11])), "Ajukan dana");
            funding = Ok(funding.Approve("Sesuai anggaran", BendaharaUser), "Verifikasi bendahara");
            funding = Ok(funding.Approve("Disetujui", KetuaUser), "Persetujuan ketua");
            funding = Ok(funding.Disburse(Accounts["Bank"], Categories["Perlengkapan & Alat"], Today.AddDays(-5), BendaharaUser), "Cairkan dana");
            Ok(FundingRealization.Save(new FundingRealization.NewRow { FundingRequestId = funding.Id, RealizationDate = Today.AddDays(-4), Description = "DP sewa sound system", Quantity = 1, Unit = "paket", Amount = 4_000_000, Receipt = Photo(10), ReceiptFileName = "kuitansi-dp.jpg", ReceiptContentType = "image/jpeg" }, UserOf(Members[11])), "Realisasi 1");
            Ok(FundingRealization.Save(new FundingRealization.NewRow { FundingRequestId = funding.Id, RealizationDate = Today.AddDays(-4), Description = "Biaya angkut", Amount = 650_000 }, UserOf(Members[11])), "Realisasi 2");
            funding = Ok(funding.SubmitSettlement(UserOf(Members[11])), "Ajukan pertanggungjawaban");
            Ok(funding.Settle(Accounts["Bank"], Categories["Pengembalian Dana"], Today.AddDays(-2), BendaharaUser), "Selesaikan pertanggungjawaban");

            // Pengajuan dana untuk pengadaan (dicairkan, dipakai di modul pengadaan).
            FundingForProcurement = Ok(FundingRequest.Save(new FundingRequest.NewRow
            {
                BudgetItemId = budgetPercussion, ProgramId = Programs["Kejurnas"].Id, Purpose = "Pembelian 2 snare drum tambahan", Amount = 15_000_000
            }, UserOf(Members[6])), "Pengajuan dana alat");
            FundingForProcurement = Ok(FundingForProcurement.Submit(UserOf(Members[6])), "Ajukan dana alat");
            FundingForProcurement = Ok(FundingForProcurement.Approve(null, BendaharaUser), "Verifikasi dana alat");
            FundingForProcurement = Ok(FundingForProcurement.Approve(null, KetuaUser), "Setujui dana alat");

            var revision = Ok(FundingRequest.Save(new FundingRequest.NewRow { ProgramId = Programs["Humas"].Id, Purpose = "Langganan aplikasi desain", Amount = 1_200_000 }, UserOf(Members[10])), "Pengajuan dana revisi");
            revision = Ok(revision.Submit(UserOf(Members[10])), "Ajukan dana revisi");
            Ok(revision.RequestRevision("Lampirkan perbandingan harga", BendaharaUser), "Minta revisi");
            var rejected = Ok(FundingRequest.Save(new FundingRequest.NewRow { ProgramId = Programs["Latihan"].Id, Purpose = "Studi banding ke Jakarta", Amount = 18_000_000 }, UserOf(Members[5])), "Pengajuan dana ditolak");
            rejected = Ok(rejected.Submit(UserOf(Members[5])), "Ajukan studi banding");
            Ok(rejected.Reject("Belum masuk anggaran", BendaharaUser), "Tolak dana");
            Ok(FundingRequest.Save(new FundingRequest.NewRow { ActivityId = Activities["Gladi"].Id, Purpose = "Konsumsi gladi bersih", Amount = 400_000 }, UserOf(Members[16])), "Pengajuan dana draft");

            // Hutang: pinjaman kas dari pembina, dicicil sebagian.
            var debt = Ok(Debt.Save(new Debt.NewRow
            {
                PeriodId = PeriodActive.Id, CreditorName = "Dr. Ir. Bambang Sutejo, M.T. (Pembina)", CreditorContact = "081234560001",
                Description = "Talangan biaya pendaftaran lomba", Amount = 3_000_000, DebtDate = Today.AddDays(-40), DueDate = Today.AddDays(50),
                LoanAccountId = Accounts["Kas"], LoanCategoryId = Categories["Pinjaman Diterima"]
            }, Admin), "Hutang");
            Ok(DebtAttachment.Save(new DebtAttachment.NewRow { DebtId = debt.Id, FileData = Pdf("Surat pernyataan pinjaman"), FileName = "pernyataan-pinjaman.pdf", FileType = "application/pdf" }, Admin), "Lampiran hutang");
            Ok(DebtPayment.Save(new DebtPayment.NewRow { DebtId = debt.Id, AccountId = Accounts["Kas"], CategoryId = Categories["Pembayaran Hutang"], Amount = 1_000_000, PaidDate = Today.AddDays(-7), Notes = "Cicilan pertama" }, Admin), "Cicilan hutang");
            Ok(Debt.Save(new Debt.NewRow { CreditorName = "Konveksi Sinar Jaya", Description = "Sisa pembayaran kaos danus", Amount = 750_000, DebtDate = Today.AddDays(-12), DueDate = Today.AddDays(18) }, Admin), "Hutang vendor");
        }

        // ---------------------------------------------------------------- Sponsorship
        private static void SeedSponsorship()
        {
            var bniContact = ExternalPartyContact.GetDataList(FilterAktif.Aktif, Parties["BNI"].Id).First(x => x.IsPrimary == ContactRank.Utama);
            SponsorBni = Ok(SponsorshipDeal.Save(new SponsorshipDeal.NewRow
            {
                ExternalPartyId = Parties["BNI"].Id, ContactId = bniContact.Id, PackageId = Packages["Gold"].Id, PeriodId = PeriodActive.Id, ActivityId = Activities["Konser"].Id,
                SponsorshipForm = SponsorshipForm.Dana, StartDate = Today.AddDays(-20), EndDate = Today.AddDays(60), PicMemberId = Members[10].Id, Notes = "Sponsor utama konser"
            }, Admin), "Sponsorship BNI");
            SponsorBni = Ok(SponsorBni.ReceivePayment(Accounts["Bank"], Categories["Sponsorship"], 5_000_000, Today.AddDays(-15), BendaharaUser), "Terima dana sponsor");
            var benefits = SponsorshipBenefit.GetDataList(SponsorBni.Id);
            Ok(benefits[0].MarkDone(Photo(11), "bukti-banner.jpg", "image/jpeg", "Logo terpasang di banner", Admin), "Benefit selesai");
            Ok(SponsorshipBenefit.Save(new SponsorshipBenefit.NewRow { SponsorshipId = SponsorBni.Id, Title = "Booth tabungan mahasiswa saat konser", Notes = "Permintaan tambahan sponsor" }, Admin), "Benefit tambahan");
            Ok(SponsorshipMou.Save(new SponsorshipMou.NewRow
            {
                SponsorshipId = SponsorBni.Id, SignedDate = Today.AddDays(-20), StartDate = Today.AddDays(-20), EndDate = Today.AddDays(60),
                Document = Pdf("MoU Sponsorship BNI - MB Gita Widya Agni"), DocumentFileName = "mou-bni.pdf", DocumentContentType = "application/pdf"
            }, Admin), "MoU BNI");

            var yamaha = Ok(SponsorshipDeal.Save(new SponsorshipDeal.NewRow
            {
                ExternalPartyId = Parties["Yamaha"].Id, PeriodId = PeriodActive.Id, ActivityId = Activities["Konser"].Id, SponsorshipForm = SponsorshipForm.Barang,
                InKindDescription = "Pinjaman 4 trumpet baru dan servis gratis alat brass", StartDate = Today.AddDays(-10), EndDate = Today.AddDays(35), PicMemberId = Members[5].Id
            }, Admin), "Sponsorship Yamaha");
            Ok(SponsorshipBenefit.Save(new SponsorshipBenefit.NewRow { SponsorshipId = yamaha.Id, Title = "Logo Yamaha di backdrop", SeqNo = 1 }, Admin), "Benefit Yamaha");
            var radio = Ok(SponsorshipDeal.Save(new SponsorshipDeal.NewRow
            {
                ExternalPartyId = Parties["Suara"].Id, PackageId = Packages["Silver"].Id, PeriodId = PeriodActive.Id, SponsorshipForm = SponsorshipForm.Media,
                InKindDescription = "Iklan radio 10 spot", Amount = 0
            }, Admin), "Sponsorship media");
            Ok(radio.Cancel("Diganti kerja sama media partner tanpa paket", Admin), "Batalkan sponsorship");
        }

        // ---------------------------------------------------------------- Pengadaan
        private static void SeedProcurement()
        {
            var request = Ok(ProcurementRequest.Save(new ProcurementRequest.NewRow
            {
                PeriodId = PeriodActive.Id, RequestedByMemberId = Members[6].Id, RequestDate = Today.AddDays(-12), Title = "Pengadaan Snare Drum & Stik",
                Reason = "Penambahan pemain battery untuk GPMB 2027", ProgramId = Programs["Kejurnas"].Id, PaymentMethod = ProcurementPaymentMethod.DibayarLangsung, NeededByDate = Today.AddDays(20),
                Items = new()
                {
                    new() { ItemName = "Marching Snare Drum 14\"", Specification = "Pearl Championship, termasuk carrier", IsAsset = ProcurementItemKind.Aset, AssetCategoryId = AssetCategories["Alat Perkusi"], Quantity = 2, Unit = "unit", EstimatedUnitPrice = 7_500_000, ExternalPartyId = Parties["Drumline"].Id },
                    new() { ItemName = "Stik snare Vic Firth", Quantity = 10, Unit = "pasang", EstimatedUnitPrice = 180_000, ExternalPartyId = Parties["Drumline"].Id }
                }
            }, UserOf(Members[6])), "Usulan pengadaan");
            var items = ProcurementItem.GetDataList(request.Id);
            var q1 = Ok(ProcurementQuotation.Save(new ProcurementQuotation.NewRow { RequestId = request.Id, ExternalPartyId = Parties["Drumline"].Id, QuotedAmount = 16_500_000, Notes = "Termasuk ongkir", File = Pdf("Penawaran CV Drumline"), FileName = "penawaran-drumline.pdf", ContentType = "application/pdf" }, Admin), "Penawaran 1");
            Ok(ProcurementQuotation.Save(new ProcurementQuotation.NewRow { RequestId = request.Id, ExternalPartyId = Parties["Yamaha"].Id, QuotedAmount = 17_200_000 }, Admin), "Penawaran 2");
            Ok(ProcurementQuotation.Save(new ProcurementQuotation.NewRow { RequestId = request.Id, ItemId = items[1].Id, VendorName = "Toko Musik Online", QuotedAmount = 1_650_000 }, Admin), "Penawaran item");
            Ok(q1.Select(Admin), "Pilih penawaran");
            request = Ok(request.Submit(UserOf(Members[6])), "Ajukan pengadaan");
            request = Ok(request.Approve("Sesuai kebutuhan", null, null, null, BendaharaUser), "Verifikasi pengadaan");
            request = Ok(request.Approve("Disetujui", Image("assets/images/gwa-logo-sm.png"), "ttd.png", "image/png", KetuaUser), "Setujui pengadaan");
            request = Ok(request.Purchase(null, Admin), "Proses pembelian");
            Ok(items[0].SetActual(7_400_000, Parties["Drumline"].Id, Admin), "Harga aktual snare");
            Ok(items[1].SetActual(175_000, null, Admin), "Harga aktual stik");
            request = Ok(request.RecordPayment(Accounts["Bank"], Categories["Perlengkapan & Alat"], 16_550_000, Today.AddDays(-6), BendaharaUser), "Bayar pengadaan");
            Ok(ProcurementReceipt.Save(new ProcurementReceipt.NewRow
            {
                RequestId = request.Id, ReceiptDate = Today.AddDays(-4), ReceivedByMemberId = Members[9].Id, Notes = "Barang diterima lengkap",
                Photo = Photo(4), PhotoFileName = "penerimaan.jpg", PhotoContentType = "image/jpeg",
                Items = new()
                {
                    new()
                    {
                        ItemId = items[0].Id, Quantity = 2, ItemCondition = ItemCondition.Baik,
                        Asset = new Asset.NewRow { AssetCode = "GWA-PRK-003", Name = "Marching Snare Drum 14\" (2026)", Brand = "Pearl", Unit = "unit", Location = "Gudang Giri Loka", RentalFeePerDay = 30_000, LateFeePerDay = 15_000 }
                    },
                    new() { ItemId = items[1].Id, Quantity = 10, ItemCondition = ItemCondition.Baik }
                }
            }, Admin), "Penerimaan barang");

            // Pengadaan dengan metode pengajuan dana.
            var second = Ok(ProcurementRequest.Save(new ProcurementRequest.NewRow
            {
                PeriodId = PeriodActive.Id, RequestedByMemberId = Members[7].Id, RequestDate = Today.AddDays(-3), Title = "Pengadaan Bendera Colour Guard",
                Reason = "Bendera lama sudah pudar", ProgramId = Programs["Kejurnas"].Id, PaymentMethod = ProcurementPaymentMethod.PengajuanDana,
                Items = new() { new() { ItemName = "Bendera silk 6 kaki", Quantity = 20, Unit = "pcs", EstimatedUnitPrice = 175_000, ExternalPartyId = Parties["Konveksi"].Id } }
            }, UserOf(Members[7])), "Usulan pengadaan 2");
            second = Ok(second.Submit(UserOf(Members[7])), "Ajukan pengadaan 2");
            Ok(second.RequestRevision("Tambahkan penawaran pembanding", BendaharaUser), "Revisi pengadaan 2");
            Ok(ProcurementRequest.Save(new ProcurementRequest.NewRow
            {
                RequestedByMemberId = Members[9].Id, RequestDate = Today, Title = "Pengadaan Lakban & Tali Rafia", Reason = "Kebutuhan logistik konser", ActivityId = Activities["Konser"].Id,
                Items = new() { new() { ItemName = "Lakban hitam", Quantity = 12, Unit = "roll", EstimatedUnitPrice = 15_000 } }
            }, UserOf(Members[9])), "Usulan pengadaan draft");

            FundingForProcurement = Ok(FundingForProcurement.Disburse(Accounts["Bank"], Categories["Perlengkapan & Alat"], Today.AddDays(-1), BendaharaUser), "Cairkan dana alat");
        }

        // ---------------------------------------------------------------- Merchandise
        private static void SeedMerchandise()
        {
            var shirt = Products["Kaos"];
            var variants = ProductVariant.GetDataList(FilterAktif.Aktif, shirt.Id);
            int Variant(string name) => variants.First(v => v.Name == name).Id;
            Ok(ProductStockMovement.Save(new ProductStockMovement.NewRow { ProductId = shirt.Id, VariantId = Variant("M"), MovementType = StockMovementType.Masuk, Quantity = 15, Notes = "Produksi batch 2" }, Admin), "Stok masuk kaos");
            Ok(ProductStockMovement.Save(new ProductStockMovement.NewRow { ProductId = Products["Gantungan"].Id, MovementType = StockMovementType.Penyesuaian, Quantity = -2, Notes = "Selisih stock opname" }, Admin), "Penyesuaian stok");
            Ok(ProductStockMovement.Save(new ProductStockMovement.NewRow { ProductId = Products["Totebag"].Id, MovementType = StockMovementType.RusakHilang, Quantity = 1, Notes = "Sobek saat display" }, Admin), "Stok rusak");

            var order1 = Ok(SalesOrder.Save(new SalesOrder.NewRow
            {
                PeriodId = PeriodActive.Id, BuyerMemberId = Members[13].Id, OrderDate = Today.AddDays(-8), PicMemberId = Members[12].Id,
                Items = new() { new() { ProductId = shirt.Id, VariantId = Variant("L"), Quantity = 1 }, new() { ProductId = Products["Gantungan"].Id, Quantity = 2 } }
            }, Admin), "Pesanan 1");
            order1 = Ok(order1.ReceivePayment(Accounts["EWallet"], Categories["Penjualan Danus"], order1.TotalAmount, Today.AddDays(-8), Admin), "Bayar pesanan 1");
            order1 = Ok(order1.SetReady(Admin), "Pesanan siap");
            Ok(order1.Finish(Admin), "Pesanan selesai");

            var order2 = Ok(SalesOrder.Save(new SalesOrder.NewRow
            {
                PeriodId = PeriodActive.Id, ActivityId = Activities["Konser"].Id, BuyerName = "Pak Hartono (orang tua pemain)", BuyerPhone = "081330001122", OrderDate = Today.AddDays(-2), PicMemberId = Members[14].Id,
                Items = new() { new() { ProductId = shirt.Id, VariantId = Variant("XL"), Quantity = 2, Notes = "Sablon nama: HARTONO" }, new() { ProductId = Products["Totebag"].Id, Quantity = 1 } }
            }, Admin), "Pesanan 2");
            Ok(order2.ReceivePayment(Accounts["Kas"], Categories["Penjualan Danus"], 100_000, Today.AddDays(-2), Admin), "DP pesanan 2");

            Ok(SalesOrder.Save(new SalesOrder.NewRow
            {
                BuyerMemberId = Members[17].Id, OrderDate = Today.AddDays(-1),
                Items = new() { new() { ProductId = Products["Jaket"].Id, Quantity = 1, Notes = "Ukuran L" }, new() { ProductId = Products["Snack"].Id, Quantity = 3 } }
            }, Admin), "Pesanan pre-order");
            var order4 = Ok(SalesOrder.Save(new SalesOrder.NewRow { BuyerMemberId = Members[18].Id, OrderDate = Today.AddDays(-5), Items = new() { new() { ProductId = shirt.Id, VariantId = Variant("S"), Quantity = 1 } } }, Admin), "Pesanan 4");
            Ok(order4.Cancel("Pembeli membatalkan", Admin), "Batalkan pesanan");
        }
    }
}
