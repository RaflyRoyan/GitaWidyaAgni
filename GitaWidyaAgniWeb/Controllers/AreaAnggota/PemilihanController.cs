using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.EVoting;
using GWAShared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>
    /// Area Anggota › Pemilihan (/AreaAnggota/Pemilihan): surat suara pemilihan yang dibuka, bukti memilih (receipt), cek bukti, dan hasil final.
    /// Pilihan terenkripsi dan tidak terhubung ke pemilih; receipt hanya ditampilkan sekali.
    /// </summary>
    public class PemilihanController : AreaAnggotaController
    {
        public record Pemilu(Election Data, ElectionEligibleVoter? Status, List<ElectionCandidate> Kandidat);

        public class Model
        {
            public List<Pemilu> Dibuka { get; set; } = new();
            public List<(Election Data, List<ElectionResult> Hasil)> Final { get; set; } = new();
        }

        public class CoblosInput
        {
            public string Key { get; set; } = "";
            public string CandidateKey { get; set; } = "";
        }

        public class CekInput
        {
            public string Key { get; set; } = "";
            public string? Receipt { get; set; }
        }

        public IActionResult Index()
        {
            var model = new Model
            {
                Dibuka = Election.GetOpenList()
                    .Select(e => new Pemilu(e, ElectionEligibleVoter.GetMyStatus(e.Id, LibUser), ElectionCandidate.GetBallot(e.Id)))
                    .ToList(),
                Final = Election.GetDataList(ElectionStatus.Finalized)
                    .OrderByDescending(e => e.FinalizedAt).Take(5)
                    .Select(e => (e, ElectionResult.GetDataList(e.Id)))
                    .ToList()
            };
            return HalamanIndex(model);
        }

        [HttpPost]
        public IActionResult Coblos([FromBody] CoblosInput m) => Aksi(u =>
        {
            var election = LibExtensions.Load<Election>(m.Key, "Pemilihan");
            var candidate = LibExtensions.Load<ElectionCandidate>(m.CandidateKey, "Kandidat");
            var receipt = ElectionVote.Cast(new ElectionVote.NewRow { ElectionId = election.Id, CandidateId = candidate.Id }, u).Ok();
            return new { receipt.Receipt, Waktu = receipt.VotedDate.ToString("dd MMM yyyy") };
        }, "Suara Anda tercatat.");

        [HttpPost]
        public IActionResult Cek([FromBody] CekInput m) => Aksi(u =>
        {
            var election = LibExtensions.Load<Election>(m.Key, "Pemilihan");
            return ElectionVote.VerifyReceipt(election.Id, (m.Receipt ?? "").Trim());
        }, "Pemeriksaan selesai.");

        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult FotoKandidat(string key)
        {
            try
            {
                var f = LibExtensions.Load<ElectionCandidate>(key, "Kandidat").GetPhoto();
                return f == null ? NotFound() : File(f.Data, f.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
