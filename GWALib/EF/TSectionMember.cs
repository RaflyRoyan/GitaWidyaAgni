using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Riwayat section anggota (migrasi 2026-11-18_section, ditulis tangan). end_date NULL = section aktif saat ini.
/// </summary>
[Table("t_section_member")]
[Index("SectionId", Name = "fk_secmember_section")]
[Index("MemberId", "EndDate", Name = "idx_secmember_member")]
public partial class TSectionMember
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("section_id")]
    public int SectionId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// NULL = section aktif anggota saat ini
    /// </summary>
    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Alasan / event perpindahan
    /// </summary>
    [Column("note")]
    [StringLength(255)]
    public string? Note { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("SectionId")]
    [InverseProperty("TSectionMembers")]
    public virtual MSection Section { get; set; } = null!;

    [ForeignKey("MemberId")]
    [InverseProperty("TSectionMembers")]
    public virtual MMember Member { get; set; } = null!;
}
