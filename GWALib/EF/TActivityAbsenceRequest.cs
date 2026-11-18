using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Pengajuan tidak ikut kegiatan, per sesi atau seluruh kegiatan (migrasi 2026-11-18_activity_absence, ditulis tangan).
/// </summary>
[Table("t_activity_absence_request")]
[Index("ActivityId", "Status", Name = "idx_absence_activity")]
[Index("MemberId", Name = "idx_absence_member")]
[Index("SessionId", Name = "fk_absence_session")]
[Index("ReviewedBy", Name = "fk_absence_reviewer")]
public partial class TActivityAbsenceRequest
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("activity_id")]
    public int ActivityId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    /// <summary>
    /// NULL = seluruh kegiatan; diisi = satu sesi presensi
    /// </summary>
    [Column("session_id")]
    public int? SessionId { get; set; }

    [Column("reason")]
    [StringLength(500)]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 1=MENUNGGU, 2=DISETUJUI, 3=DITOLAK, 4=DITARIK
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("review_note")]
    [StringLength(500)]
    public string? ReviewNote { get; set; }

    [Column("reviewed_by")]
    public int? ReviewedBy { get; set; }

    [Column("reviewed_at", TypeName = "datetime")]
    public DateTime? ReviewedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    public virtual TActivity Activity { get; set; } = null!;

    [ForeignKey("MemberId")]
    public virtual MMember Member { get; set; } = null!;

    [ForeignKey("SessionId")]
    public virtual TAttendanceSession? Session { get; set; }

    [ForeignKey("ReviewedBy")]
    public virtual MUser? ReviewedByNavigation { get; set; }
}
