using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Kontak PIC pihak luar
/// </summary>
[Table("m_external_party_contact")]
[Index("ExternalPartyId", "IsActive", "IsPrimary", Name = "ix_contact_party")]
public partial class MExternalPartyContact
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("external_party_id")]
    public int ExternalPartyId { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("position")]
    [StringLength(100)]
    public string? Position { get; set; }

    [Column("phone")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [Column("email")]
    [StringLength(150)]
    public string? Email { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | maksimal satu kontak utama aktif per mitra
    /// </summary>
    [Column("is_primary")]
    public int IsPrimary { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
    /// </summary>
    [Column("is_active")]
    public int IsActive { get; set; }

    [Column("notes")]
    [StringLength(500)]
    public string? Notes { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ExternalPartyId")]
    [InverseProperty("MExternalPartyContacts")]
    public virtual MExternalParty ExternalParty { get; set; } = null!;

    [InverseProperty("Contact")]
    public virtual ICollection<TBorrowRequest> TBorrowRequests { get; set; } = new List<TBorrowRequest>();

    [InverseProperty("Contact")]
    public virtual ICollection<TSponsorship> TSponsorships { get; set; } = new List<TSponsorship>();
}
