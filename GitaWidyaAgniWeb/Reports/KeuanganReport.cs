using GWALib.Lib.Transaksi.Keuangan;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// Laporan keuangan (DevExpress) dari GWALib FinancialReport, hanya transaksi Posted: buku kas (landscape), arus kas,
    /// realisasi anggaran (landscape), dan rekap iuran (landscape). Data dan penanda tangan disusun Models/Keuangan/KeuanganCetak.
    /// </summary>
    public class KeuanganReport : DocumentReport
    {
        private KeuanganReport(string title, bool landscape) : base(title, landscape, totalPages: true)
        {
        }

        private static string Range(DateOnly from, DateOnly to) => from == to ? Date(from) : $"{Date(from)} s.d. {Date(to)}";
        private static string Pct(decimal value) => value.ToString("0.#", ID) + "%";
        private static string Month(int year, int month) => new DateTime(year, month, 1).ToString("MMMM yyyy", ID);

        // Tabel kosong: satu baris berisi teks di kolom kedua.
        private static List<Row> OrEmpty(List<Row> rows, string text)
        {
            if (rows.Count == 0)
            {
                rows.Add(new Row { C2 = text });
            }
            return rows;
        }

        private void Close(ReportBrand brand, List<Signer> signers)
        {
            Signatures(brand.City, DateTime.Today, signers, knownLast: false, gap: 16);
        }

        // ================================================================= BUKU KAS
        public static KeuanganReport BukuKas(FinancialReport.CashBook d, ReportBrand brand, List<Signer> signers)
        {
            var range = Range(d.From, d.To);
            var r = new KeuanganReport("Buku Kas " + range, true);
            r.Letterhead(brand);
            r.TitleBlock(new[] { "BUKU KAS", range }, false, underlineFirst: true);
            r.Fields(new (string, string?)[]
            {
                ("Akun", d.Accounts.Count == 1 ? d.Accounts[0].Name : $"{d.Accounts.Count} akun"),
                ("Saldo awal", Rp(d.Opening)),
                ("Pemasukan", Rp(d.Income)),
                ("Pengeluaran", Rp(d.Expense)),
                ("Saldo akhir", Rp(d.Closing))
            }, labelWidth: 120);

            var money = 125f;
            var widths = new float[] { 34, 82, 150, 150, r.W - 34 - 82 - 150 - 150 - 3 * money, money, money, money };
            var aligns = new[] { C, C, L, L, L, R, R, R };
            foreach (var a in d.Accounts)
            {
                var rows = new List<Row> { new() { C5 = "Saldo awal", C8 = Rp(a.Opening), Bold = true } };
                rows.AddRange(a.Lines.Select((x, k) => new Row
                {
                    C1 = (k + 1).ToString(),
                    C2 = x.Date.ToString("dd/MM/yyyy"),
                    C3 = x.Number,
                    C4 = x.Category,
                    C5 = x.Description ?? "-",
                    C6 = x.Income == 0 ? "" : Num(x.Income),
                    C7 = x.Expense == 0 ? "" : Num(x.Expense),
                    C8 = Num(x.Balance)
                }));
                if (a.Lines.Count == 0)
                {
                    rows.Add(new Row { C5 = "Tidak ada transaksi" });
                }
                rows.Add(new Row { C5 = "Jumlah / saldo akhir", C6 = Num(a.Income), C7 = Num(a.Expense), C8 = Rp(a.Closing), Bold = true, Shade = true });
                r.Table(new[] { "NO", "TANGGAL", "NOMOR", "KATEGORI", "KETERANGAN", "MASUK", "KELUAR", "SALDO" }, widths, aligns, rows,
                    heading: "AKUN: " + a.Name.ToUpper(ID) + (a.Detail == null ? "" : " (" + a.Detail + ")"), fontSize: 10f);
            }
            if (d.Accounts.Count > 1)
            {
                var rows = d.Accounts.Select((a, k) => new Row
                {
                    C1 = (k + 1).ToString(), C2 = a.Name, C3 = Rp(a.Opening), C4 = Rp(a.Income), C5 = Rp(a.Expense), C6 = Rp(a.Closing)
                }).ToList();
                rows.Add(new Row { C2 = "TOTAL", C3 = Rp(d.Opening), C4 = Rp(d.Income), C5 = Rp(d.Expense), C6 = Rp(d.Closing), Bold = true, Shade = true });
                var w = (r.W - 34 - 260) / 4;
                r.Table(new[] { "NO", "AKUN", "SALDO AWAL", "MASUK", "KELUAR", "SALDO AKHIR" }, new[] { 34, 260f, w, w, w, w }, new[] { C, L, R, R, R, R }, rows,
                    heading: "RINGKASAN PER AKUN", fontSize: 10.5f);
            }
            r.Paragraphs("- Hanya transaksi berstatus Diposting. Saldo awal = saldo awal akun ditambah transaksi sebelum tanggal mulai.", false, "CATATAN", left: 0);
            r.Close(brand, signers);
            return r;
        }

        // ================================================================= ARUS KAS
        public static KeuanganReport ArusKas(FinancialReport.CashFlow d, ReportBrand brand, List<Signer> signers)
        {
            var range = Range(d.From, d.To);
            var r = new KeuanganReport("Laporan Arus Kas " + range, false);
            r.Letterhead(brand);
            r.TitleBlock(new[] { "LAPORAN ARUS KAS", range }, false, underlineFirst: true);
            r.Fields(new (string, string?)[] { ("Akun", d.Account), ("Rentang", range) }, labelWidth: 90);

            var widths = new float[] { 34, r.W - 34 - 105 - 150, 105, 150 };
            var aligns = new[] { C, L, C, R };
            var headers = new[] { "NO", "KATEGORI", "TRANSAKSI", "JUMLAH" };
            List<Row> Lines(List<FinancialReport.AmountRow> list, string total, decimal sum)
            {
                var rows = OrEmpty(list.Select((x, k) => new Row { C1 = (k + 1).ToString(), C2 = x.Name, C3 = x.Count.ToString(), C4 = Rp(x.Amount) }).ToList(), "Tidak ada");
                rows.Add(new Row { C2 = total, C4 = Rp(sum), Bold = true, Shade = true });
                return rows;
            }
            r.Table(headers, widths, aligns, Lines(d.Income, "Total pemasukan", d.TotalIncome), heading: "A. PEMASUKAN", fontSize: 10.5f);
            r.Table(headers, widths, aligns, Lines(d.Expense, "Total pengeluaran", d.TotalExpense), heading: "B. PENGELUARAN", fontSize: 10.5f);

            var net = d.TotalIncome - d.TotalExpense;
            r.Table(null, new[] { r.W - 180, 180f }, new[] { L, R }, new List<Row>
            {
                new() { C1 = "Saldo awal", C2 = Rp(d.Opening) },
                new() { C1 = "Total pemasukan", C2 = Rp(d.TotalIncome) },
                new() { C1 = "Total pengeluaran", C2 = Rp(-d.TotalExpense) },
                new() { C1 = net >= 0 ? "Surplus" : "Defisit", C2 = Rp(net), Bold = true },
                new() { C1 = "Saldo akhir", C2 = Rp(d.Closing), Bold = true, Shade = true }
            }, heading: "C. RINGKASAN", fontSize: 11f);

            var mw = (r.W - 150) / 3;
            var months = d.Months.Select(x => new Row
            {
                C1 = Month(x.Year, x.Month), C2 = Num(x.Income), C3 = Num(x.Expense), C4 = (x.Net < 0 ? "-" : "") + Num(Math.Abs(x.Net)), Shade = x.Net < 0
            }).ToList();
            r.Table(new[] { "BULAN", "PEMASUKAN", "PENGELUARAN", "SELISIH" }, new[] { 150f, mw, mw, mw }, new[] { L, R, R, R }, months,
                heading: "D. PER BULAN", fontSize: 10.5f);
            r.Paragraphs("- Hanya transaksi berstatus Diposting.\n- Baris berlatar pada tabel per bulan menandai pengeluaran lebih besar dari pemasukan.", false, "CATATAN", left: 0);
            r.Close(brand, signers);
            return r;
        }

        // ================================================================= REALISASI ANGGARAN
        public static KeuanganReport RealisasiAnggaran(FinancialReport.BudgetRealization d, ReportBrand brand, List<Signer> signers)
        {
            var r = new KeuanganReport("Realisasi Anggaran " + d.Name, true);
            r.Letterhead(brand);
            r.TitleBlock(new[] { "LAPORAN REALISASI ANGGARAN", d.Name }, false, underlineFirst: true);
            r.Fields(new (string, string?)[]
            {
                ("Periode", d.Period),
                ("Status anggaran", d.Status),
                ("Total RAB", Rp(d.Amount)),
                ("Realisasi", Rp(d.Realized) + " (" + Pct(d.Percent) + ")"),
                ("Sisa", Rp(d.Remaining))
            }, labelWidth: 140);

            var money = 130f;
            var widths = new float[] { 34, 150, r.W - 34 - 150 - 140 - 3 * money - 70, 140, money, money, money, 70 };
            var rows = d.Lines.Select((x, k) => new Row
            {
                C1 = (k + 1).ToString(),
                C2 = x.Category,
                C3 = x.Name + (string.IsNullOrWhiteSpace(x.Program) ? "" : "\nProgram: " + x.Program),
                C4 = $"{x.Quantity.ToString("0.##", ID)} {x.Unit}".Trim() + "\n@ " + Rp(x.UnitPrice),
                C5 = Num(x.Amount),
                C6 = Num(x.Realized),
                C7 = (x.Remaining < 0 ? "-" : "") + Num(Math.Abs(x.Remaining)),
                C8 = Pct(x.Percent),
                Shade = x.Remaining < 0
            }).ToList();
            if (rows.Count == 0)
            {
                rows.Add(new Row { C3 = "Anggaran belum punya item" });
            }
            rows.Add(new Row { C3 = "TOTAL", C5 = Rp(d.Amount), C6 = Rp(d.Realized), C7 = Rp(d.Remaining), C8 = Pct(d.Percent), Bold = true, Shade = true });
            r.Table(new[] { "NO", "KATEGORI", "URAIAN", "VOLUME", "RAB", "REALISASI", "SISA", "%" }, widths, new[] { C, L, L, L, R, R, R, C }, rows,
                heading: "A. RAB DAN REALISASI PER ITEM", fontSize: 10f);

            if (d.Unbudgeted.Count > 0)
            {
                var other = d.Unbudgeted.Select((x, k) => new Row { C1 = (k + 1).ToString(), C2 = x.Name, C3 = x.Count.ToString(), C4 = Rp(x.Amount) }).ToList();
                other.Add(new Row { C2 = "Total", C4 = Rp(d.Unbudgeted.Sum(x => x.Amount)), Bold = true, Shade = true });
                r.Table(new[] { "NO", "KATEGORI", "TRANSAKSI", "JUMLAH" }, new[] { 34, r.W - 34 - 110 - 180, 110f, 180f }, new[] { C, L, C, R }, other,
                    heading: "B. PENGELUARAN DI LUAR ANGGARAN (" + d.Period.ToUpper(ID) + ")", fontSize: 10.5f);
            }
            r.Paragraphs("- Realisasi = pengeluaran Diposting yang memakai item anggaran, dikurangi pemasukan yang memakai item yang sama (mis. pengembalian sisa dana).\n"
                + "- Baris berlatar menandai realisasi yang melebihi RAB.", false, "CATATAN", left: 0);
            r.Close(brand, signers);
            return r;
        }

        // ================================================================= REKAP IURAN
        public static KeuanganReport RekapIuran(FinancialReport.DuesRecap d, string section, ReportBrand brand, List<Signer> signers)
        {
            var r = new KeuanganReport("Rekap Iuran " + d.Name, true);
            r.Letterhead(brand);
            r.TitleBlock(new[] { "REKAP IURAN ANGGOTA", d.Name }, false, underlineFirst: true);
            r.Fields(new (string, string?)[]
            {
                ("Nominal", Rp(d.Amount) + " · " + d.Frequency),
                ("Periode", d.Period ?? "-"),
                ("Section", section),
                ("Terkumpul", Rp(d.Received)),
                ("Tunggakan", Rp(d.Outstanding))
            }, labelWidth: 110);

            var money = 150f;
            var n = 112f;
            var periods = OrEmpty(d.Periods.Select((x, k) => new Row
            {
                C1 = (k + 1).ToString(), C2 = x.BillPeriod, C3 = x.Bills.ToString(), C4 = x.Paid.ToString(), C5 = x.Waived.ToString(), C6 = x.Unpaid.ToString(),
                C7 = Num(x.Received), C8 = Num(x.Outstanding)
            }).ToList(), "Belum ada tagihan");
            periods.Add(new Row
            {
                C2 = "TOTAL", C3 = d.Periods.Sum(x => x.Bills).ToString(), C4 = d.Periods.Sum(x => x.Paid).ToString(), C5 = d.Periods.Sum(x => x.Waived).ToString(),
                C6 = d.Periods.Sum(x => x.Unpaid).ToString(), C7 = Rp(d.Received), C8 = Rp(d.Outstanding), Bold = true, Shade = true
            });
            r.Table(new[] { "NO", "PERIODE TAGIHAN", "TAGIHAN", "LUNAS", "DIBEBASKAN", "BELUM", "TERKUMPUL", "TUNGGAKAN" },
                new[] { 34, r.W - 34 - 4 * n - 2 * money, n, n, n, n, money, money }, new[] { C, L, C, C, C, C, R, R }, periods,
                heading: "A. PER PERIODE TAGIHAN", fontSize: 10.5f);

            var members = OrEmpty(d.Members.Select((x, k) => new Row
            {
                C1 = (k + 1).ToString(),
                C2 = x.Name + (string.IsNullOrWhiteSpace(x.MemberNumber) ? "" : "\n" + x.MemberNumber),
                C3 = x.Section ?? "-",
                C4 = x.Bills.ToString(),
                C5 = x.Paid + (x.Waived > 0 ? $" (+{x.Waived} bebas)" : ""),
                C6 = x.Unpaid.ToString(),
                C7 = x.Outstanding == 0 ? "-" : Num(x.Outstanding),
                C8 = x.UnpaidPeriods.Count == 0 ? "-" : string.Join(", ", x.UnpaidPeriods),
                Shade = x.Unpaid > 0
            }).ToList(), "Belum ada tagihan");
            r.Table(new[] { "NO", "NAMA / NIA", "SECTION", "TAGIHAN", "LUNAS", "BELUM", "TUNGGAKAN", "PERIODE BELUM DIBAYAR" },
                new[] { 34, 210, 120, 85, 110, 70, 120, r.W - 34 - 210 - 120 - 85 - 110 - 70 - 120 }, new[] { C, L, L, C, C, C, R, L }, members,
                heading: "B. PER ANGGOTA", fontSize: 10f);
            r.Paragraphs("- Tagihan Dibebaskan tidak dihitung sebagai tunggakan.\n- Baris berlatar menandai anggota yang masih punya tunggakan; urut dari tunggakan terbesar.", false, "CATATAN", left: 0);
            r.Close(brand, signers);
            return r;
        }
    }
}
