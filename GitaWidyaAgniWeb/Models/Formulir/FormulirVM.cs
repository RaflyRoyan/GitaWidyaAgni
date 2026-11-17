using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Anggota;
using GWALib.Lib.Transaksi.Formulir;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Formulir
{
    /// <summary>
    /// Formulir › Formulir: formulir evaluasi kegiatan, pendaftaran rekrutmen, dan survei umum; penyusun pertanyaan; jawaban dan ringkasan.
    /// Draft → Dibuka → Ditutup. Setelah ada jawaban, pertanyaan, jenis, referensi, dan mode anonim terkunci.
    /// </summary>
    public class FormulirVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Form.GetDataList().Select(x => new Row
            {
                EncDecrKey = x.EncDecrKey,
                Title = x.Title,
                Jenis = EnumHelper.Text(x.FormType),
                Referensi = x.ActivityTitle ?? x.RecruitmentTitle,
                Anonim = x.IsAnonymous == IdentityMode.Anonim,
                OpenAt = x.OpenAt,
                CloseAt = x.CloseAt,
                QuestionCount = x.QuestionCount,
                ResponseCount = x.ResponseCount,
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status),
                InsDate = x.InsDate
            }).OrderByDescending(x => x.InsDate).ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string Jenis { get; set; } = "";
            public string? Referensi { get; set; }
            public bool Anonim { get; set; }
            public DateTime? OpenAt { get; set; }
            public DateTime? CloseAt { get; set; }
            public int QuestionCount { get; set; }
            public int ResponseCount { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public DateTime InsDate { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<FormType>();
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan();
            public List<GwaVM.OpsiItem> RekrutmenOpsi { get; set; } = Recruitment.GetDataList().Select(x => new GwaVM.OpsiItem(x.Id, x.Title)).ToList();
            public bool Terkunci { get; set; }
        }

        public class Edit : Add
        {
            public string StatusText { get; set; } = "";
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<Form>(encKey, "Formulir");
                StatusText = EnumHelper.Text(obj.Status) + " · " + obj.ResponseCount + " jawaban";
                Terkunci = obj.ResponseCount > 0;
                Row = new NewRow
                {
                    FormType = (int)obj.FormType,
                    ActivityId = obj.ActivityId,
                    RecruitmentId = obj.RecruitmentId,
                    Title = obj.Title,
                    Description = obj.Description,
                    Anonim = obj.IsAnonymous == IdentityMode.Anonim,
                    OpenAt = obj.OpenAt,
                    CloseAt = obj.CloseAt
                };
            }
        }

        public class NewRow
        {
            [Display(Name = "Jenis Formulir")]
            public int FormType { get; set; } = (int)GWAShared.Enums.FormType.SurveiUmum;

            [Display(Name = "Kegiatan (Evaluasi)")]
            public int? ActivityId { get; set; }

            [Display(Name = "Rekrutmen Anggota (kosong = untuk rekrutmen tim pelaksana)")]
            public int? RecruitmentId { get; set; }

            [Required(ErrorMessage = "Judul harus diisi")]
            [StringLength(255, ErrorMessage = "Judul maksimal 255 karakter")]
            [Display(Name = "Judul")]
            public string? Title { get; set; }

            [Display(Name = "Deskripsi")]
            public string? Description { get; set; }

            [Display(Name = "Anonim (identitas tidak disimpan)")]
            public bool Anonim { get; set; }

            [Display(Name = "Dibuka Mulai")]
            public DateTime? OpenAt { get; set; }

            [Display(Name = "Ditutup Pada")]
            public DateTime? CloseAt { get; set; }
        }

        // Penyusun pertanyaan (modal).
        public class Pertanyaan
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public bool Terkunci { get; set; }
            public List<GwaVM.OpsiItem> TipeOpsi { get; set; } = GwaVM.Opsi<QuestionType>();
            public List<FormQuestion> Rows { get; set; }

            public Pertanyaan(string encKey)
            {
                var obj = LibExtensions.Load<Form>(encKey, "Formulir");
                Key = obj.EncDecrKey;
                Judul = obj.Title + " · " + EnumHelper.Text(obj.Status);
                Terkunci = obj.ResponseCount > 0;
                Rows = FormQuestion.GetDataList(obj.Id).OrderBy(x => x.Seq).ToList();
            }
        }

        public class PertanyaanInput
        {
            public string Key { get; set; } = "";
            public string? QuestionKey { get; set; }
            public int Seq { get; set; }
            public string? QuestionText { get; set; }
            public int QuestionType { get; set; }
            public string? Options { get; set; }
            public bool Wajib { get; set; }
        }

        // Jawaban + ringkasan (modal).
        public class Jawaban
        {
            public string Judul { get; set; } = "";
            public bool Anonim { get; set; }
            public List<Form.QuestionSummary> Ringkasan { get; set; }
            public List<FormResponse> Respon { get; set; }

            public Jawaban(string encKey)
            {
                var obj = LibExtensions.Load<Form>(encKey, "Formulir");
                Judul = obj.Title + " · " + obj.ResponseCount + " jawaban";
                Anonim = obj.IsAnonymous == IdentityMode.Anonim;
                Ringkasan = obj.GetSummary().OrderBy(x => x.Seq).ToList();
                Respon = FormResponse.GetDataList(obj.Id).OrderByDescending(x => x.SubmittedAt).ToList();
            }
        }

        public class JawabanDetail
        {
            public string Judul { get; set; } = "";
            public List<FormAnswer> Rows { get; set; }
            public JawabanDetail(string encKey)
            {
                var obj = LibExtensions.Load<FormResponse>(encKey, "Jawaban");
                Judul = (obj.MemberName ?? obj.ApplicantName ?? "Anonim") + " · " + obj.SubmittedAt.ToString("dd MMM yyyy HH:mm");
                Rows = FormAnswer.GetDataList(obj.Id).OrderBy(x => x.QuestionSeq).ToList();
            }
        }

        public class Method
        {
            public static Form Load(string? key) => LibExtensions.Load<Form>(key, "Formulir");

            public static void Simpan(NewRow input, string user)
                => Form.Save(new Form.NewRow
                {
                    FormType = (FormType)input.FormType,
                    ActivityId = input.ActivityId,
                    RecruitmentId = input.RecruitmentId,
                    Title = input.Title ?? "",
                    Description = input.Description,
                    IsAnonymous = input.Anonim ? IdentityMode.Anonim : IdentityMode.Teridentifikasi,
                    OpenAt = input.OpenAt,
                    CloseAt = input.CloseAt
                }, user).Ok();

            public static void Update(string key, NewRow input, string user)
            {
                var obj = Load(key);
                obj.FormType = (FormType)input.FormType;
                obj.ActivityId = input.ActivityId;
                obj.RecruitmentId = input.RecruitmentId;
                obj.Title = input.Title ?? "";
                obj.Description = input.Description;
                obj.IsAnonymous = input.Anonim ? IdentityMode.Anonim : IdentityMode.Teridentifikasi;
                obj.OpenAt = input.OpenAt;
                obj.CloseAt = input.CloseAt;
                obj.Update(user).Ok();
            }

            private static List<string>? Pilihan(string? text)
                => string.IsNullOrWhiteSpace(text) ? null
                    : text.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();

            public static void SimpanPertanyaan(PertanyaanInput input, string user)
            {
                var wajib = input.Wajib ? AnswerRequirement.Wajib : AnswerRequirement.Opsional;
                if (string.IsNullOrEmpty(input.QuestionKey))
                {
                    var form = Load(input.Key);
                    FormQuestion.Save(new FormQuestion.NewRow
                    {
                        FormId = form.Id,
                        Seq = input.Seq,
                        QuestionText = input.QuestionText ?? "",
                        QuestionType = (QuestionType)input.QuestionType,
                        Options = Pilihan(input.Options),
                        IsRequired = wajib
                    }, user).Ok();
                    return;
                }
                var q = LibExtensions.Load<FormQuestion>(input.QuestionKey, "Pertanyaan");
                q.Seq = input.Seq;
                q.QuestionText = input.QuestionText ?? "";
                q.QuestionType = (QuestionType)input.QuestionType;
                q.Options = Pilihan(input.Options) ?? new List<string>();
                q.IsRequired = wajib;
                q.Update(user).Ok();
            }

            public static FormResponse LoadRespon(string? key) => LibExtensions.Load<FormResponse>(key, "Jawaban");
            public static FormAnswer LoadJawaban(string? key) => LibExtensions.Load<FormAnswer>(key, "Jawaban");
        }
    }
}
