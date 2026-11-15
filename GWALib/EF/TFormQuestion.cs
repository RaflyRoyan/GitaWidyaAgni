using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_form_question")]
[Index("FormId", "Seq", Name = "ix_question_form")]
public partial class TFormQuestion
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("form_id")]
    public int FormId { get; set; }

    [Column("seq")]
    public int Seq { get; set; }

    [Column("question_text")]
    [StringLength(500)]
    public string QuestionText { get; set; } = null!;

    /// <summary>
    /// 1=TEKS_SINGKAT, 2=PARAGRAF, 3=PILIHAN_GANDA, 4=KOTAK_CENTANG, 5=SKALA_1_5, 6=TANGGAL, 7=FILE
    /// </summary>
    [Column("question_type")]
    public int QuestionType { get; set; }

    /// <summary>
    /// Daftar pilihan untuk tipe 3 dan 4, mis. [&quot;Ya&quot;,&quot;Tidak&quot;]
    /// </summary>
    [Column("options", TypeName = "json")]
    public string? Options { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
    /// </summary>
    [Column("is_required")]
    public int IsRequired { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("FormId")]
    [InverseProperty("TFormQuestions")]
    public virtual TForm Form { get; set; } = null!;

    [InverseProperty("Question")]
    public virtual ICollection<TFormAnswer> TFormAnswers { get; set; } = new List<TFormAnswer>();
}
