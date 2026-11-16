using GWALib.General;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Formulir
{
    /// <summary>
    /// Respons formulir (t_form_response) beserta jawabannya, disimpan sekaligus dan tidak diubah sesudahnya. Formulir harus Open dan
    /// dalam rentang waktu.
    /// - Submit (login): formulir anonim tidak menyimpan identitas sama sekali (member_id dan ins_by NULL, jadi tidak ada batas satu
    ///   respons). Formulir tidak anonim: satu respons per user, member_id = anggota user bila ada; formulir rekrutmen tim pelaksana
    ///   (PendaftaranRekrutmen tanpa rekrutmen) wajib anggota.
    /// - SubmitApplicant (tanpa login): formulir PendaftaranRekrutmen dengan rekrutmen; pendaftar harus dari rekrutmen itu, email cocok,
    ///   dan statusnya belum final. Satu respons per pendaftar.
    /// Hapus ditolak bila dipakai lamaran tim pelaksana; jawaban ikut terhapus.
    /// </summary>
    public partial class FormResponse : BaseLib
    {
        public class NewRow
        {
            public int FormId { get; set; }
            public List<FormAnswer.NewRow> Answers { get; set; } = new();
        }
        public class ApplicantRow : NewRow
        {
            public int ApplicantId { get; set; }
            // Pencocokan sederhana karena pendaftar tidak login.
            public string ApplicantEmail { get; set; } = null!;
        }

        public int Id { get; set; }
        public int FormId { get; private set; }
        public string FormTitle { get; private set; } = null!;
        public int? MemberId { get; private set; }
        public string? MemberName { get; private set; }
        public int? ApplicantId { get; private set; }
        public string? ApplicantName { get; private set; }
        public DateTime SubmittedAt { get; private set; }

        private static IQueryable<EF.TFormResponse> _Querys
        {
            get
            {
                return DBClass.GetContext().TFormResponses.Select(x => new EF.TFormResponse
                {
                    Id = x.Id,
                    FormId = x.FormId,
                    Form = new EF.TForm { Id = x.FormId, Title = x.Form.Title },
                    MemberId = x.MemberId,
                    Member = x.MemberId == null ? null : new EF.MMember { Id = x.Member!.Id, Fullname = x.Member.Fullname },
                    ApplicantId = x.ApplicantId,
                    Applicant = x.ApplicantId == null ? null : new EF.TRecruitmentApplicant { Id = x.Applicant!.Id, Fullname = x.Applicant.Fullname },
                    SubmittedAt = x.SubmittedAt,
                    InsBy = x.InsBy
                });
            }
        }
        public FormResponse()
        {
        }
        public FormResponse(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<FormResponse>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal FormResponse(EF.TFormResponse row)
        {
            SetData(row);
        }
        private void SetData(EF.TFormResponse row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<FormResponse>();
            FormId = row.FormId;
            FormTitle = row.Form?.Title ?? string.Empty;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname;
            ApplicantId = row.ApplicantId;
            ApplicantName = row.Applicant?.Fullname;
            SubmittedAt = row.SubmittedAt;
        }
    }
}
