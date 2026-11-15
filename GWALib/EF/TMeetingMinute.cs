using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_meeting_minutes")]
[Index("ChairedByMemberId", Name = "fk_minutes_chair")]
[Index("FinalizedBy", Name = "fk_minutes_finalizer")]
[Index("NotulisMemberId", Name = "fk_minutes_notulis")]
[Index("SessionId", Name = "uq_minutes_session", IsUnique = true)]
public partial class TMeetingMinute
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("session_id")]
    public int SessionId { get; set; }

    /// <summary>
    /// Pimpinan rapat
    /// </summary>
    [Column("chaired_by_member_id")]
    public int? ChairedByMemberId { get; set; }

    /// <summary>
    /// Penulis notulen
    /// </summary>
    [Column("notulis_member_id")]
    public int? NotulisMemberId { get; set; }

    [Column("agenda")]
    [StringLength(2000)]
    public string? Agenda { get; set; }

    /// <summary>
    /// Isi / jalannya rapat
    /// </summary>
    [Column("content")]
    public string Content { get; set; } = null!;

    /// <summary>
    /// Keputusan rapat
    /// </summary>
    [Column("decisions")]
    [StringLength(2000)]
    public string? Decisions { get; set; }

    /// <summary>
    /// Tindak lanjut + penanggung jawab
    /// </summary>
    [Column("follow_up")]
    [StringLength(2000)]
    public string? FollowUp { get; set; }

    /// <summary>
    /// Lampiran (foto papan tulis, materi, dll.)
    /// </summary>
    [Column("file_data", TypeName = "mediumblob")]
    public byte[]? FileData { get; set; }

    [Column("file_name")]
    [StringLength(255)]
    public string? FileName { get; set; }

    [Column("content_type")]
    [StringLength(100)]
    public string? ContentType { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=FINAL
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("finalized_by")]
    public int? FinalizedBy { get; set; }

    [Column("finalized_at", TypeName = "datetime")]
    public DateTime? FinalizedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ChairedByMemberId")]
    [InverseProperty("TMeetingMinuteChairedByMembers")]
    public virtual MMember? ChairedByMember { get; set; }

    [ForeignKey("FinalizedBy")]
    [InverseProperty("TMeetingMinutes")]
    public virtual MUser? FinalizedByNavigation { get; set; }

    [ForeignKey("NotulisMemberId")]
    [InverseProperty("TMeetingMinuteNotulisMembers")]
    public virtual MMember? NotulisMember { get; set; }

    [ForeignKey("SessionId")]
    [InverseProperty("TMeetingMinute")]
    public virtual TAttendanceSession Session { get; set; } = null!;
}
