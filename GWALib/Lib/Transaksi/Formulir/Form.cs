using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Formulir
{
    /// <summary>
    /// Formulir (t_form). EvaluasiKegiatan wajib menyebut kegiatan. PendaftaranRekrutmen boleh menyebut rekrutmen anggota (jawaban dari
    /// pendaftar tanpa login, FormResponse.SubmitApplicant) atau dikosongkan untuk rekrutmen tim pelaksana (t_committee_recruitment.form_id),
    /// dan tidak boleh anonim. SurveiUmum tanpa referensi. Kolom referensi yang tidak berlaku dikosongkan.
    /// Alur: Draft → Open → Closed; Closed bisa dibuka lagi bila waktu tutup kosong / belum lewat. Open butuh minimal 1 pertanyaan.
    /// Setelah ada jawaban, pertanyaan, jenis, referensi dan mode anonim terkunci. Hapus hanya Draft yang tidak dipakai (pertanyaan ikut terhapus).
    /// </summary>
    public partial class Form : BaseLib
    {
        public class NewRow
        {
            public FormType FormType { get; set; } = FormType.SurveiUmum;
            public int? ActivityId { get; set; }
            public int? RecruitmentId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public IdentityMode IsAnonymous { get; set; } = IdentityMode.Teridentifikasi;
            public DateTime? OpenAt { get; set; }
            public DateTime? CloseAt { get; set; }
            // Opsional: pertanyaan yang disimpan bersama formulir.
            public List<FormQuestion.NewRow>? Questions { get; set; }
        }
        public record OptionCount(string Option, int Count);
        // Ringkasan jawaban per pertanyaan: jumlah per pilihan (pilihan ganda / kotak centang / skala) dan rata-rata skala.
        public record QuestionSummary(int QuestionId, int Seq, string QuestionText, QuestionType QuestionType, int AnswerCount,
            decimal? Average, List<OptionCount> Options);

        public int Id { get; set; }
        // Jenis, referensi dan mode anonim terkunci setelah ada jawaban.
        public FormType FormType { get; set; }
        public int? ActivityId { get; set; }
        public string? ActivityTitle { get; private set; }
        public int? RecruitmentId { get; set; }
        public string? RecruitmentTitle { get; private set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public IdentityMode IsAnonymous { get; set; }
        public DateTime? OpenAt { get; set; }
        public DateTime? CloseAt { get; set; }
        // Hanya-baca: diubah lewat Open / Close.
        public FormStatus Status { get; private set; }
        public int QuestionCount { get; private set; }
        public int ResponseCount { get; private set; }
        public DateTime InsDate { get; private set; }

        private static IQueryable<EF.TForm> _Querys
        {
            get
            {
                return DBClass.GetContext().TForms.Select(x => new EF.TForm
                {
                    Id = x.Id,
                    FormType = x.FormType,
                    ActivityId = x.ActivityId,
                    Activity = x.ActivityId == null ? null : new EF.TActivity { Id = x.Activity!.Id, Title = x.Activity.Title },
                    RecruitmentId = x.RecruitmentId,
                    Recruitment = x.RecruitmentId == null ? null : new EF.TRecruitment { Id = x.Recruitment!.Id, Title = x.Recruitment.Title },
                    Title = x.Title,
                    Description = x.Description,
                    IsAnonymous = x.IsAnonymous,
                    OpenAt = x.OpenAt,
                    CloseAt = x.CloseAt,
                    Status = x.Status,
                    InsDate = x.InsDate,
                    TFormQuestions = x.TFormQuestions.Select(q => new EF.TFormQuestion { Id = q.Id }).ToList(),
                    TFormResponses = x.TFormResponses.Select(r => new EF.TFormResponse { Id = r.Id }).ToList()
                });
            }
        }
        public Form()
        {
        }
        public Form(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Form>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Form(EF.TForm row)
        {
            SetData(row);
        }
        private void SetData(EF.TForm row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Form>();
            FormType = (FormType)row.FormType;
            ActivityId = row.ActivityId;
            ActivityTitle = row.Activity?.Title;
            RecruitmentId = row.RecruitmentId;
            RecruitmentTitle = row.Recruitment?.Title;
            Title = row.Title;
            Description = row.Description;
            IsAnonymous = (IdentityMode)row.IsAnonymous;
            OpenAt = row.OpenAt;
            CloseAt = row.CloseAt;
            Status = (FormStatus)row.Status;
            QuestionCount = row.TFormQuestions.Count;
            ResponseCount = row.TFormResponses.Count;
            InsDate = row.InsDate;
        }
    }
}
