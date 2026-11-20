using GWALib.Lib.Transaksi.Information;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// QR tanda tangan elektronik di blok tanda tangan DocumentReport. BaseController.Laporan membuka konteks (user pencetak + URL publik)
    /// selama laporan dibuat; DocumentReport.Signatures(qrType: ...) meminta token GWALib DocumentSignature untuk setiap penanda tangan
    /// bernama (kecuali Signer.Wet) dan mencetak QR {PublicUrl}/ttd/{token} di tempat tanda tangan basah.
    /// Tanpa konteks (mis. ekspor LPJ saat finalisasi) atau bila penerbitan gagal, blok tanda tangan tetap kosong untuk tanda tangan basah.
    /// </summary>
    public static class TandaTanganQr
    {
        private record Context(string User, string BaseUrl);
        private static readonly AsyncLocal<Context?> Current = new();

        /// <summary>Jenis dokumen (kode t_document_signature.doc_type) → nama tampilan di halaman verifikasi.</summary>
        public static readonly IReadOnlyDictionary<string, string> Jenis = new Dictionary<string, string>
        {
            ["LAPORAN_KEUANGAN"] = "Laporan keuangan",
            ["RAPOR_ANGGOTA"] = "Rapor anggota",
            ["REKAP_POIN"] = "Rekap poin keaktifan",
            ["REKAP_KEHADIRAN"] = "Rekap kehadiran",
            ["LAPORAN_INVENTARIS"] = "Laporan inventaris",
            ["INVOICE_PEMINJAMAN"] = "Invoice peminjaman alat",
            ["KUITANSI_PEMINJAMAN"] = "Kuitansi peminjaman alat",
            ["SURAT_KELUAR"] = "Surat keluar"
        };

        public static IDisposable Mulai(string user, string baseUrl)
        {
            var previous = Current.Value;
            Current.Value = new Context(user, baseUrl.TrimEnd('/'));
            return new Scope(() => Current.Value = previous);
        }

        /// <summary>URL QR per penanda tangan (urutan sama); null = tanpa QR (tanpa nama, Wet, tanpa konteks, atau gagal).</summary>
        public static List<string?> Terbitkan(string docType, string title, string? reference, DateOnly date, IReadOnlyList<DocumentReport.Signer> signers)
        {
            var result = signers.Select(_ => (string?)null).ToList();
            var ctx = Current.Value;
            if (ctx == null)
            {
                return result;
            }
            var index = new List<int>();
            var rows = new List<DocumentSignature.IssueRow>();
            for (var i = 0; i < signers.Count; i++)
            {
                var s = signers[i];
                if (s.Wet || string.IsNullOrWhiteSpace(s.Name))
                {
                    continue;
                }
                index.Add(i);
                rows.Add(new DocumentSignature.IssueRow(docType, title, reference, s.Role.Replace("\n", " ").Trim().TrimEnd(','), s.Name!, s.Number, date));
            }
            if (rows.Count == 0)
            {
                return result;
            }
            try
            {
                var tokens = DocumentSignature.Issue(rows, ctx.User);
                for (var k = 0; k < tokens.Count; k++)
                {
                    result[index[k]] = $"{ctx.BaseUrl}/ttd/{tokens[k]}";
                }
            }
            catch (ArgumentException)
            {
                // User tidak valid / data penanda tangan tidak lengkap: cetak tanpa QR (tempat tanda tangan basah).
            }
            return result;
        }

        private sealed class Scope : IDisposable
        {
            private Action? _onDispose;
            public Scope(Action onDispose) => _onDispose = onDispose;
            public void Dispose()
            {
                _onDispose?.Invoke();
                _onDispose = null;
            }
        }
    }
}
