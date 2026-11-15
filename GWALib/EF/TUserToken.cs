using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_user_token")]
[Index("UserId", "TokenType", Name = "ix_token_user")]
[Index("TokenHash", Name = "uq_token_hash", IsUnique = true)]
public partial class TUserToken
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// 1=RESET_PASSWORD, 2=VERIFY_EMAIL, 3=REFRESH
    /// </summary>
    [Column("token_type")]
    public int TokenType { get; set; }

    /// <summary>
    /// Simpan hash (SHA-256), bukan token mentah
    /// </summary>
    [Column("token_hash")]
    [StringLength(64)]
    public string TokenHash { get; set; } = null!;

    [Column("expires_at", TypeName = "datetime")]
    public DateTime ExpiresAt { get; set; }

    [Column("used_at", TypeName = "datetime")]
    public DateTime? UsedAt { get; set; }

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
    [InverseProperty("TUserTokens")]
    public virtual MUser User { get; set; } = null!;
}
