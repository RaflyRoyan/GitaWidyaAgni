using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_form_answer")]
[Index("QuestionId", Name = "fk_answer_question")]
[Index("ResponseId", "QuestionId", Name = "uq_answer", IsUnique = true)]
public partial class TFormAnswer
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("response_id")]
    public int ResponseId { get; set; }

    [Column("question_id")]
    public int QuestionId { get; set; }

    /// <summary>
    /// Teks / pilihan / angka skala / tanggal
    /// </summary>
    [Column("answer_text")]
    [StringLength(2000)]
    public string? AnswerText { get; set; }

    /// <summary>
    /// Untuk pertanyaan tipe FILE
    /// </summary>
    [Column("answer_file", TypeName = "mediumblob")]
    public byte[]? AnswerFile { get; set; }

    [Column("answer_file_name")]
    [StringLength(255)]
    public string? AnswerFileName { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("QuestionId")]
    [InverseProperty("TFormAnswers")]
    public virtual TFormQuestion Question { get; set; } = null!;

    [ForeignKey("ResponseId")]
    [InverseProperty("TFormAnswers")]
    public virtual TFormResponse Response { get; set; } = null!;
}
