namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// Laporan Peminjaman Alat (DevExpress): invoice (rincian sewa, denda, pembayaran, sisa tagihan) dan kuitansi
    /// (bukti terima uang, satu pembayaran atau gabungan semua pembayaran). Data disusun Models/Inventaris/PeminjamanCetak.
    /// </summary>
    public class PeminjamanReport : DocumentReport
    {
        public class Data
        {
            public string RequestNumber { get; set; } = "";
            public string BorrowerName { get; set; } = "";
            public string? BorrowerType { get; set; }
            public string? Institution { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public string Purpose { get; set; } = "";
            public string? Guarantee { get; set; }
            public DateTime BorrowDate { get; set; }
            public DateTime ExpectedReturnDate { get; set; }
            public DateTime? ActualReturnDate { get; set; }
            public string Scheme { get; set; } = "";
            public bool FixedRate { get; set; }
            public bool Voluntary { get; set; }
            public string Status { get; set; } = "";
            public bool Returned { get; set; }
            public decimal TotalRentalFee { get; set; }
            public decimal TotalLateFee { get; set; }
            public decimal LateFeeWaived { get; set; }
            public string? WaiveNote { get; set; }
            public decimal TotalDue { get; set; }
            public decimal PaidAmount { get; set; }
            public List<Item> Items { get; set; } = new();
            public List<Payment> Payments { get; set; } = new();
            /// <summary>Rekening tujuan transfer (bank / e-wallet aktif), satu baris per rekening.</summary>
            public List<string> Accounts { get; set; } = new();
            public Signer Treasurer { get; set; } = new("Bendahara", null, null);
            /// <summary>Tanda air invoice (LUNAS / DIBATALKAN / DITOLAK); null = tanpa tanda air.</summary>
            public string? Stamp { get; set; }
        }

        public record Item(string Asset, string? Units, int Quantity, int RentalDays, decimal RentalFeePerDay, decimal RentalFee,
            int LateDays, decimal LateFeePerDay, decimal LateFee);

        public record Payment(int Id, string Number, DateOnly Date, string Category, string Account, decimal Amount);

        private PeminjamanReport(string title) : base(title, totalPages: true)
        {
        }

        // ================================================================= INVOICE
        public static PeminjamanReport Invoice(Data d, ReportBrand brand)
        {
            var r = new PeminjamanReport("Invoice " + d.RequestNumber);
            r.Letterhead(brand);
            r.TitleBlock(new[] { "INVOICE PEMINJAMAN ALAT", "No. " + d.RequestNumber }, false, underlineFirst: true);

            // Baris opsional yang kosong tidak dicetak supaya invoice muat satu halaman; tanggal invoice ada di tanda tangan.
            var contact = string.Join(" · ", new[] { d.Phone, d.Email }.Where(x => !string.IsNullOrWhiteSpace(x)));
            var fields = new List<(string, string?)>
            {
                ("Kepada", d.BorrowerName + (string.IsNullOrWhiteSpace(d.BorrowerType) ? "" : $" ({d.BorrowerType})"))
            };
            if (!string.IsNullOrWhiteSpace(d.Institution))
            {
                fields.Add(("Instansi", d.Institution));
            }
            if (contact.Length > 0)
            {
                fields.Add(("Telepon / Email", contact));
            }
            fields.Add(("Keperluan", d.Purpose));
            fields.Add(("Waktu pinjam", $"{When(d.BorrowDate)} s.d. {When(d.ExpectedReturnDate)}"));
            fields.Add(("Dikembalikan", d.ActualReturnDate == null ? $"Belum ({d.Status})" : When(d.ActualReturnDate.Value)));
            fields.Add(("Skema biaya", d.Scheme));
            if (!string.IsNullOrWhiteSpace(d.Guarantee))
            {
                fields.Add(("Jaminan", d.Guarantee.Trim()));
            }
            r.Fields(fields, labelWidth: 130);

            // Rincian sewa per barang, lalu denda keterlambatan per barang.
            var rows = new List<Row>();
            var no = 0;
            foreach (var x in d.Items)
            {
                rows.Add(new Row
                {
                    C1 = (++no).ToString(),
                    C2 = "Sewa " + x.Asset + (string.IsNullOrWhiteSpace(x.Units) ? "" : "\nUnit: " + x.Units),
                    C3 = x.Quantity.ToString(),
                    C4 = d.FixedRate ? x.RentalDays + " hari" : "-",
                    C5 = d.FixedRate ? Num(x.RentalFeePerDay) : "-",
                    C6 = Num(x.RentalFee)
                });
            }
            foreach (var x in d.Items.Where(x => x.LateFee > 0))
            {
                rows.Add(new Row
                {
                    C1 = (++no).ToString(),
                    C2 = "Denda keterlambatan " + x.Asset,
                    C3 = x.Quantity.ToString(),
                    C4 = x.LateDays + " hari",
                    C5 = Num(x.LateFeePerDay),
                    C6 = Num(x.LateFee)
                });
            }
            if (rows.Count == 0)
            {
                rows.Add(new Row { C2 = "Belum ada barang" });
            }
            r.Table(new[] { "NO", "URAIAN", "JML", "HARI", "TARIF / HARI (Rp)", "JUMLAH (Rp)" },
                new float[] { 34, r.W - 404, 50, 70, 120, 130 }, new[] { C, L, C, C, R, R }, rows, heading: "RINCIAN BIAYA", fontSize: 10.5f);

            var summary = new List<Row>
            {
                new() { C2 = "Total sewa", C3 = Rp(d.TotalRentalFee) },
                new() { C2 = "Total denda keterlambatan", C3 = Rp(d.TotalLateFee) }
            };
            if (d.LateFeeWaived > 0)
            {
                summary.Add(new Row { C2 = "Pembebasan denda", C3 = Rp(-d.LateFeeWaived) });
            }
            summary.Add(new Row { C2 = "Total tagihan", C3 = Rp(d.TotalDue), Bold = true });
            // Satu baris per pembayaran (nomor transaksi untuk dicocokkan dengan kuitansi).
            summary.AddRange(d.Payments.Select(x => new Row { C2 = $"Dibayar {x.Date.ToString("d MMM yyyy", ID)} ({x.Number})", C3 = Rp(-x.Amount) }));
            summary.Add(new Row { C2 = "SISA TAGIHAN", C3 = Rp(Math.Max(0, d.TotalDue - d.PaidAmount)), Bold = true });
            r.Table(null, new float[] { r.W - 420, 290, 130 }, new[] { L, L, R }, summary, borders: false, fontSize: SIZE, rowHeight: 20);

            var notes = new List<string>();
            if (d.Voluntary)
            {
                notes.Add("- Skema seikhlasnya: biaya sewa tidak ditagih; kontribusi sukarela diterima dengan terima kasih.");
            }
            else if (!d.FixedRate)
            {
                notes.Add("- Peminjaman ini tanpa biaya sewa.");
            }
            if (!d.Returned)
            {
                notes.Add("- Keterlambatan pengembalian dikenai denda sesuai tarif per hari per barang; denda dihitung saat barang kembali.");
            }
            if (d.LateFeeWaived > 0 && !string.IsNullOrWhiteSpace(d.WaiveNote))
            {
                notes.Add("- Pembebasan denda: " + d.WaiveNote.Trim());
            }
            if (notes.Count > 0)
            {
                r.Paragraphs(string.Join("\n", notes), false, "CATATAN", left: 0);
            }
            // Rekening tujuan rata kiri (paragraf rata kanan-kiri meregangkan nomor rekening).
            if (d.TotalDue - d.PaidAmount > 0 && d.Accounts.Count > 0)
            {
                r.Fields(new (string, string?)[]
                {
                    ("Transfer ke", string.Join("\n", d.Accounts)),
                    ("Berita transfer", d.RequestNumber)
                }, labelWidth: 130, heading: notes.Count > 0 ? null : "PEMBAYARAN");
            }

            r.Signatures(brand.City, System.DateTime.Today, new List<Signer> { d.Treasurer }, knownLast: false, gap: 10);
            if (d.Stamp != null)
            {
                r.Stamp(d.Stamp);
            }
            return r;
        }

        // ================================================================= KUITANSI
        /// <summary>Kuitansi satu pembayaran (payment) atau gabungan semua pembayaran (payment null).</summary>
        public static PeminjamanReport Kuitansi(Data d, Payment? payment, ReportBrand brand)
        {
            var payments = payment == null ? d.Payments : new List<Payment> { payment };
            var amount = payments.Sum(x => x.Amount);
            var number = payment?.Number ?? (payments.Count == 1 ? payments[0].Number : d.RequestNumber);
            var date = payment?.Date ?? payments.Max(x => x.Date);

            var r = new PeminjamanReport("Kuitansi " + number);
            r.Letterhead(brand);
            r.TitleBlock(new[] { "KUITANSI", "No. " + number }, false, underlineFirst: true);

            var categories = string.Join(", ", payments.Select(x => x.Category).Distinct());
            var items = string.Join(", ", d.Items.Select(x => $"{x.Asset} ({x.Quantity})"));
            r.Fields(new (string, string?)[]
            {
                ("Telah terima dari", d.BorrowerName + (string.IsNullOrWhiteSpace(d.Institution) ? "" : " — " + d.Institution)),
                ("Uang sejumlah", Capital(Terbilang(amount)) + " rupiah"),
                ("Untuk pembayaran", $"{categories} — peminjaman alat No. {d.RequestNumber} ({d.Purpose})" + (items.Length > 0 ? "; barang: " + items : "")),
                ("Diterima melalui", string.Join(", ", payments.Select(x => x.Account).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())),
                ("Tanggal", Date(date))
            }, labelWidth: 140);
            r.Spacer(10);
            r.Table(null, new float[] { 240 }, new[] { DevExpress.XtraPrinting.TextAlignment.MiddleCenter }, new List<Row> { new() { C1 = Rp(amount), Bold = true, Shade = true } },
                fontSize: 15f, rowHeight: 34);

            if (payment == null && payments.Count > 1)
            {
                var rows = payments.Select((x, k) => new Row
                {
                    C1 = (k + 1).ToString(),
                    C2 = x.Date.ToString("d MMM yyyy", ID),
                    C3 = x.Number,
                    C4 = x.Category,
                    C5 = Num(x.Amount)
                }).ToList();
                r.Table(new[] { "NO", "TANGGAL", "NOMOR TRANSAKSI", "KATEGORI", "JUMLAH (Rp)" },
                    new float[] { 34, 95, 170, r.W - 429, 130 }, new[] { C, C, L, L, R }, rows, heading: "RINCIAN PEMBAYARAN", fontSize: 10.5f);
            }

            if (!d.Voluntary && d.TotalDue > 0)
            {
                var rest = Math.Max(0, d.TotalDue - d.PaidAmount);
                r.Paragraphs($"Total tagihan {Rp(d.TotalDue)} · total dibayar {Rp(d.PaidAmount)} · sisa {Rp(rest)}" + (rest == 0 ? " (LUNAS)" : ""), false, left: 0);
            }

            r.Signatures(brand.City, date.ToDateTime(TimeOnly.MinValue), new List<Signer>
            {
                new("Penyetor,", d.BorrowerName, null),
                new("Penerima,\n" + d.Treasurer.Role, d.Treasurer.Name, d.Treasurer.Number)
            }, knownLast: false);
            return r;
        }

        // ================================================================= TEKS
        private static string When(System.DateTime value) => value.ToString("d MMMM yyyy HH:mm", ID);

        private static string Capital(string text) => text.Length == 0 ? text : char.ToUpper(text[0], ID) + text[1..];

    }
}
