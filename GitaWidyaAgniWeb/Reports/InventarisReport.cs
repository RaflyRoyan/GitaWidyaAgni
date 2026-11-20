using GWALib.Lib.Transaksi.Inventaris;
using GWAShared.Extensions;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// Laporan inventaris (DevExpress, A4 landscape) dari GWALib InventoryReport: ringkasan kondisi, daftar aset per kategori
    /// (jumlah per kondisi, dipinjam / perawatan, lokasi, nilai perolehan), lalu riwayat perawatan dalam rentang tanggal.
    /// Data dan penanda tangan: Models/Inventaris/InventarisCetak.
    /// </summary>
    public class InventarisReport : DocumentReport
    {
        private InventarisReport(string title) : base(title, landscape: true, totalPages: true)
        {
        }

        public static InventarisReport Create(InventoryReport.Inventory d, ReportBrand brand, List<Signer> signers)
        {
            var range = d.From == d.To ? Date(d.From) : $"{Date(d.From)} s.d. {Date(d.To)}";
            var r = new InventarisReport("Laporan Inventaris " + DateTime.Today.ToString("yyyy-MM-dd"));
            r.Letterhead(brand);
            r.TitleBlock(new[] { "LAPORAN INVENTARIS", "Per " + Date(DateTime.Today) }, false, underlineFirst: true);
            r.Fields(new (string, string?)[]
            {
                ("Kategori", d.Category ?? "Semua kategori"),
                ("Jumlah aset", $"{d.Assets.Count} jenis, {Num(d.Quantity)} barang / unit"),
                ("Kondisi", $"Baik {Num(d.Baik)} · Rusak ringan {Num(d.RusakRingan)} · Rusak berat {Num(d.RusakBerat)} · Hilang {Num(d.Hilang)}"),
                ("Nilai perolehan", Rp(d.Cost)),
                ("Perawatan", $"{d.Maintenance.Count} kegiatan ({range}), biaya {Rp(d.MaintenanceCost)}")
            }, labelWidth: 120);

            // A. Daftar aset per kategori
            var widths = new float[] { 34, 118, 0, 85, 60, 115, 85, 215 };
            widths[2] = r.W - widths.Sum();
            var aligns = new[] { C, C, L, C, C, C, C, L };
            var headers = new[] { "NO", "KODE", "NAMA ASET", "JUMLAH", "BAIK", "RUSAK R / B", "HILANG", "KETERANGAN" };
            var first = true;
            foreach (var g in d.Assets.GroupBy(x => x.Category))
            {
                var rows = g.Select((x, k) => new Row
                {
                    C1 = (k + 1).ToString(),
                    C2 = x.Code,
                    C3 = x.Name + (string.IsNullOrWhiteSpace(x.Brand) ? "" : " (" + x.Brand + ")"),
                    C4 = Num(x.Quantity) + " " + x.Unit,
                    C5 = Num(x.Baik),
                    C6 = $"{Num(x.RusakRingan)} / {Num(x.RusakBerat)}",
                    C7 = Num(x.Hilang),
                    C8 = Note(x),
                    Shade = x.RusakBerat > 0 || x.Hilang > 0
                }).ToList();
                rows.Add(new Row
                {
                    C3 = "Jumlah " + g.Key, C4 = Num(g.Sum(x => x.Quantity)), C5 = Num(g.Sum(x => x.Baik)),
                    C6 = $"{Num(g.Sum(x => x.RusakRingan))} / {Num(g.Sum(x => x.RusakBerat))}", C7 = Num(g.Sum(x => x.Hilang)),
                    C8 = g.Any(x => x.Cost != null) ? "Nilai " + Rp(g.Sum(x => x.Cost ?? 0)) : "", Bold = true, Shade = true
                });
                r.Table(headers, widths, aligns, rows, heading: (first ? "A. DAFTAR ASET — " : "") + g.Key.ToUpper(ID), fontSize: 10f);
                first = false;
            }
            if (d.Assets.Count == 0)
            {
                r.Table(headers, widths, aligns, new List<Row> { new() { C3 = "Tidak ada aset" } }, heading: "A. DAFTAR ASET", fontSize: 10f);
            }

            // B. Riwayat perawatan
            var mw = new float[] { 34, 82, 0, 230, 120, 105, 90 };
            mw[2] = r.W - mw.Sum();
            var mrows = d.Maintenance.Select((x, k) => new Row
            {
                C1 = (k + 1).ToString(),
                C2 = x.Date.ToString("dd/MM/yyyy"),
                C3 = x.AssetCode + " · " + x.Asset + (x.UnitCode == null ? "" : " (unit " + x.UnitCode + ")"),
                C4 = x.Description,
                C5 = string.IsNullOrWhiteSpace(x.Vendor) ? "-" : x.Vendor,
                C6 = x.Cost == 0 ? "-" : Num(x.Cost),
                C7 = x.Status.GetDescription()
            }).ToList();
            if (mrows.Count == 0)
            {
                mrows.Add(new Row { C3 = "Tidak ada perawatan dalam rentang ini" });
            }
            else
            {
                mrows.Add(new Row { C3 = "Jumlah biaya", C6 = Num(d.MaintenanceCost), Bold = true, Shade = true });
            }
            r.Table(new[] { "NO", "TANGGAL", "ASET", "PEKERJAAN", "VENDOR", "BIAYA", "STATUS" }, mw, new[] { C, C, L, L, L, R, C }, mrows,
                heading: "B. RIWAYAT PERAWATAN (" + range + ")", fontSize: 10f);

            r.Paragraphs("- Aset yang sudah dihapuskan tidak dicantumkan. Aset per unit dihitung dari unitnya; aset cukup jumlah memakai kondisi asetnya.\n"
                + "- Baris berlatar = ada barang rusak berat atau hilang.", false, "CATATAN", left: 0);
            r.Signatures(brand.City, DateTime.Today, signers, knownLast: false, gap: 16);
            return r;
        }

        private static string Note(InventoryReport.AssetRow x)
        {
            var parts = new List<string>();
            if (x.Borrowed > 0)
            {
                parts.Add($"dipinjam {Num(x.Borrowed)}");
            }
            if (x.InMaintenance > 0)
            {
                parts.Add($"perawatan {Num(x.InMaintenance)}");
            }
            if (!string.IsNullOrWhiteSpace(x.Location))
            {
                parts.Add(x.Location!);
            }
            if (x.AcquisitionDate != null || x.Cost != null)
            {
                parts.Add("perolehan " + (x.AcquisitionDate?.ToString("MM/yyyy") ?? "") + (x.Cost != null ? " " + Rp(x.Cost.Value) : ""));
            }
            return parts.Count == 0 ? "-" : string.Join("; ", parts);
        }
    }
}
