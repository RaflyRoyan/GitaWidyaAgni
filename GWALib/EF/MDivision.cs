using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_division")]
[Index("Name", Name = "uq_division_name", IsUnique = true)]
public partial class MDivision
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// mis. Humas, Acara, Tim Futsal Putra
    /// </summary>
    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 1=DIVISI/BIDANG, 2=TIM
    /// </summary>
    [Column("division_type")]
    public int DivisionType { get; set; }

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
    /// </summary>
    [Column("is_active")]
    public int IsActive { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [InverseProperty("Division")]
    public virtual ICollection<TProgram> TPrograms { get; set; } = new List<TProgram>();

    [InverseProperty("Division")]
    public virtual ICollection<TActivity> TActivities { get; set; } = new List<TActivity>();

    [InverseProperty("Division")]
    public virtual ICollection<TDivisionMember> TDivisionMembers { get; set; } = new List<TDivisionMember>();

    [InverseProperty("Division")]
    public virtual ICollection<TManagementAssignment> TManagementAssignments { get; set; } = new List<TManagementAssignment>();

    [InverseProperty("Division")]
    public virtual ICollection<TProgramTask> TProgramTasks { get; set; } = new List<TProgramTask>();
}
