using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_activity_log")]
[Index("EntityName", "EntityId", Name = "ix_log_entity")]
[Index("UserId", "InsDate", Name = "ix_log_user")]
public partial class TActivityLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("module")]
    [StringLength(50)]
    public string Module { get; set; } = null!;

    /// <summary>
    /// CREATE, UPDATE, DELETE, LOGIN, APPROVE, ...
    /// </summary>
    [Column("action")]
    [StringLength(50)]
    public string Action { get; set; } = null!;

    /// <summary>
    /// Nama tabel
    /// </summary>
    [Column("entity_name")]
    [StringLength(100)]
    public string? EntityName { get; set; }

    [Column("entity_id")]
    public int? EntityId { get; set; }

    [Column("old_value", TypeName = "json")]
    public string? OldValue { get; set; }

    [Column("new_value", TypeName = "json")]
    public string? NewValue { get; set; }

    [Column("ip_address")]
    [StringLength(45)]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    [StringLength(255)]
    public string? UserAgent { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("TActivityLogs")]
    public virtual MUser? User { get; set; }
}
