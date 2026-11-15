using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_form.form_type
    /// </summary>
    public enum FormType
    {
        [Description("Evaluasi Kegiatan")] EvaluasiKegiatan = 1,
        [Description("Pendaftaran Rekrutmen")] PendaftaranRekrutmen = 2,
        [Description("Survei Umum")] SurveiUmum = 3
    }
}
