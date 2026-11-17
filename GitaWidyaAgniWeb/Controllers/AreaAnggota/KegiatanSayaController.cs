using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GWALib.Lib.Transaksi.Program;
using GWALib.Lib.Transaksi.TimPelaksana;
using GWAShared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>
    /// Area Anggota › Kegiatan (/AreaAnggota/KegiatanSaya): daftar jadi peserta kegiatan, melamar tim pelaksana yang sedang rekrutmen,
    /// dan melihat tugas di tim pelaksana. Semua data memakai anggota login.
    /// </summary>
    public class KegiatanSayaController : AreaAnggotaController
    {
        public class Model
        {
            public List<Activity> Kegiatan { get; set; } = new();
            public Dictionary<int, ActivityParticipant> Daftar { get; set; } = new();
            public List<Rekrutmen> Rekrutmen { get; set; } = new();
            public List<CommitteeApplication> Lamaran { get; set; } = new();
            public List<CommitteeMember> Tim { get; set; } = new();
            public Dictionary<int, string> JudulKegiatan { get; set; } = new();
        }

        public record Rekrutmen(CommitteeRecruitment Data, string Kegiatan, List<GwaVM.OpsiItem> Seksi, bool PerluFormulir);

        public class LamarInput
        {
            public int RecruitmentId { get; set; }
            public int SectionChoice1Id { get; set; }
            public int? SectionChoice2Id { get; set; }
            public string? Reason { get; set; }
        }

        public IActionResult Index()
        {
            var saya = SayaAtauNull();
            if (saya == null)
            {
                return HalamanIndex(null);
            }
            var dari = DateTime.Today.AddDays(-1);
            var kegiatan = Activity.GetDataList(status: ProgramStatus.Planned, from: dari)
                .Concat(Activity.GetDataList(status: ProgramStatus.Ongoing))
                .GroupBy(x => x.Id).Select(g => g.First())
                .OrderBy(x => x.StartAt).Take(50).ToList();
            var judul = Activity.GetDataList().ToDictionary(x => x.Id, x => x.Title);
            var model = new Model
            {
                Kegiatan = kegiatan,
                Daftar = ActivityParticipant.GetDataList(memberId: saya.Id).GroupBy(x => x.ActivityId).ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.InsDate).First()),
                Lamaran = CommitteeApplication.GetDataList(memberId: saya.Id).OrderByDescending(x => x.InsDate).ToList(),
                Tim = CommitteeMember.GetDataList(memberId: saya.Id).Where(x => x.Status == CommitteeMemberStatus.Aktif).ToList(),
                JudulKegiatan = judul
            };
            model.Rekrutmen = CommitteeRecruitment.GetOpenList().Select(r => new Rekrutmen(
                r,
                judul.TryGetValue(r.ActivityId, out var t) ? t : "Kegiatan #" + r.ActivityId,
                CommitteeSection.GetDataList(r.ActivityId).OrderBy(s => s.SeqNo).Select(s => new GwaVM.OpsiItem(s.Id, s.Name)).ToList(),
                r.FormId.HasValue)).ToList();
            return HalamanIndex(model);
        }

        [HttpPost]
        public IActionResult Daftar([FromBody] GwaVM.KeyInput i) => Aksi(u =>
        {
            var kegiatan = LibExtensions.Load<Activity>(i.Key, "Kegiatan");
            ActivityParticipant.Save(new ActivityParticipant.NewRow { ActivityId = kegiatan.Id, MemberId = Saya().Id, ParticipantRole = ActivityParticipant.DEFAULT_ROLE }, u).Ok();
        }, "Pendaftaran peserta tersimpan.");

        [HttpPost]
        public IActionResult Batal([FromBody] GwaVM.KeyInput i) => Aksi(u =>
        {
            var p = LibExtensions.Load<ActivityParticipant>(i.Key, "Pendaftaran");
            if (p.MemberId != Saya().Id)
            {
                throw new ArgumentException(ACCESS_DENIED_MESSAGE);
            }
            p.Cancel(u).Ok();
        }, "Pendaftaran dibatalkan.");

        [HttpPost]
        public IActionResult Lamar([FromBody] LamarInput m) => Aksi(u =>
        {
            // Rekrutmen dengan formulir: lampirkan jawaban formulir milik anggota ini (diisi dulu di menu Formulir).
            var rekrutmen = CommitteeRecruitment.GetOpenList().FirstOrDefault(x => x.Id == m.RecruitmentId)
                ?? throw new ArgumentException("Rekrutmen tim pelaksana tidak sedang dibuka.");
            int? responseId = null;
            if (rekrutmen.FormId.HasValue)
            {
                responseId = GWALib.Lib.Transaksi.Formulir.FormResponse.GetMyResponse(rekrutmen.FormId.Value, u)?.Id
                    ?? throw new ArgumentException("Isi dulu formulir pendaftaran tim pelaksana di menu Formulir, lalu kirim lamaran.");
            }
            CommitteeApplication.Apply(new CommitteeApplication.NewRow
            {
                RecruitmentId = m.RecruitmentId,
                MemberId = Saya().Id,
                SectionChoice1Id = m.SectionChoice1Id,
                SectionChoice2Id = m.SectionChoice2Id,
                Reason = m.Reason,
                FormResponseId = responseId
            }, u).Ok();
        }, "Lamaran tim pelaksana terkirim.");

        [HttpPost]
        public IActionResult Mundur([FromBody] GwaVM.KeyInput i) => Aksi(u =>
        {
            var a = LibExtensions.Load<CommitteeApplication>(i.Key, "Lamaran");
            if (a.MemberId != Saya().Id)
            {
                throw new ArgumentException(ACCESS_DENIED_MESSAGE);
            }
            a.Withdraw(u).Ok();
        }, "Lamaran ditarik.");
    }
}
