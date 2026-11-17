using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GWALib.Lib.Transaksi.Formulir;
using GWAShared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>
    /// Area Anggota › Formulir (/AreaAnggota/FormulirSaya): isi formulir yang dibuka (evaluasi, survei, pendaftaran tim pelaksana)
    /// dan kirim aspirasi. Formulir anonim tidak menyimpan identitas; formulir lain hanya bisa diisi sekali.
    /// </summary>
    public class FormulirSayaController : AreaAnggotaController
    {
        public class Model
        {
            public List<(Form Form, bool SudahIsi)> Formulir { get; set; } = new();
            public List<Aspiration> Aspirasi { get; set; } = new();
            public List<GwaVM.OpsiItem> KategoriAspirasi { get; set; } = GwaVM.Opsi<AspirationCategory>();
        }

        public class IsiModel
        {
            public Form Form { get; set; } = null!;
            public List<FormQuestion> Pertanyaan { get; set; } = new();
        }

        public IActionResult Index()
        {
            var model = new Model
            {
                // Formulir pendaftaran rekrutmen anggota diisi pelamar (tanpa login), bukan di sini.
                Formulir = Form.GetOpenList()
                    .Where(f => !(f.FormType == FormType.PendaftaranRekrutmen && f.RecruitmentId.HasValue))
                    .Select(f => (f, f.IsAnonymous == IdentityMode.Teridentifikasi && FormResponse.GetMyResponse(f.Id, LibUser) != null))
                    .ToList(),
                Aspirasi = Aspiration.GetMyList(LibUser).OrderByDescending(x => x.InsDate).ToList()
            };
            return HalamanIndex(model);
        }

        // Halaman isi formulir.
        public IActionResult Isi(string key)
        {
            try
            {
                var form = LibExtensions.Load<Form>(key, "Formulir");
                if (form.Status != FormStatus.Open)
                {
                    return RedirectToAction(nameof(Index));
                }
                return View($"{URLView}Isi.cshtml", new IsiModel { Form = form, Pertanyaan = FormQuestion.GetDataList(form.Id).OrderBy(q => q.Seq).ToList() });
            }
            catch (ArgumentException)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // Jawaban dikirim sebagai FormData: q{id} (teks / pilihan, berulang untuk kotak centang) dan f{id} (berkas).
        [HttpPost]
        public async Task<IActionResult> Kirim(string key)
        {
            var answers = new List<FormAnswer.NewRow>();
            var form = Request.Form;
            Form? obj = null;
            try
            {
                obj = LibExtensions.Load<Form>(key, "Formulir");
                foreach (var q in FormQuestion.GetDataList(obj.Id))
                {
                    var row = new FormAnswer.NewRow { QuestionId = q.Id };
                    if (q.QuestionType == QuestionType.File)
                    {
                        var file = form.Files.GetFile("f" + q.Id);
                        row.FileData = await BacaBerkasAsync(file);
                        row.FileName = file?.FileName;
                    }
                    else if (q.QuestionType == QuestionType.KotakCentang)
                    {
                        row.Choices = form["q" + q.Id].Where(v => !string.IsNullOrEmpty(v)).Select(v => v!).ToList();
                    }
                    else
                    {
                        row.AnswerText = form["q" + q.Id].FirstOrDefault();
                    }
                    answers.Add(row);
                }
            }
            catch (ArgumentException e)
            {
                return JsonError(e);
            }
            return Aksi(u => { FormResponse.Submit(new FormResponse.NewRow { FormId = obj!.Id, Answers = answers }, u).Ok(); }, "Jawaban terkirim. Terima kasih!");
        }

        // ---------- Aspirasi
        [HttpPost]
        public async Task<IActionResult> KirimAspirasi([FromForm] int category, [FromForm] string? title, [FromForm] string? content, [FromForm] bool anonim, IFormFile? lampiran)
        {
            var bytes = await BacaBerkasAsync(lampiran);
            return Aksi(u => Aspiration.Submit(new Aspiration.NewRow
            {
                Category = (AspirationCategory)category,
                Title = title ?? "",
                Content = content ?? "",
                IsAnonymous = anonim ? IdentityMode.Anonim : IdentityMode.Teridentifikasi,
                Attachment = bytes,
                AttachmentFileName = lampiran?.FileName
            }, u).Ok(), "Aspirasi terkirim.");
        }

        [HttpPost]
        public IActionResult HapusAspirasi([FromBody] GwaVM.KeyInput i)
            => Aksi(u => { LibExtensions.Load<Aspiration>(i.Key, "Aspirasi").Delete(u).Ok(); }, "Aspirasi dihapus.");
    }
}
