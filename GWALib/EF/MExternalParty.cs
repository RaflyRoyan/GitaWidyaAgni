using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Pihak luar: brand, organisasi lain, vendor
/// </summary>
[Table("m_external_party")]
[Index("PartyType", "Status", Name = "ix_party_type")]
[Index("Name", Name = "uq_party_name", IsUnique = true)]
public partial class MExternalParty
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 1=PERUSAHAAN/BRAND, 2=INSTANSI_PEMERINTAH, 3=MEDIA_PARTNER, 4=KOMUNITAS/ORGANISASI, 5=VENDOR/TOKO, 6=LEMBAGA_PENDIDIKAN, 7=LAINNYA
    /// </summary>
    [Column("party_type")]
    public int PartyType { get; set; }

    [Column("industry")]
    [StringLength(100)]
    public string? Industry { get; set; }

    [Column("address")]
    [StringLength(255)]
    public string? Address { get; set; }

    [Column("city")]
    [StringLength(100)]
    public string? City { get; set; }

    [Column("phone")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [Column("email")]
    [StringLength(150)]
    public string? Email { get; set; }

    [Column("website")]
    [StringLength(255)]
    public string? Website { get; set; }

    [Column("instagram")]
    [StringLength(100)]
    public string? Instagram { get; set; }

    [Column("logo", TypeName = "mediumblob")]
    public byte[]? Logo { get; set; }

    [Column("logo_file_name")]
    [StringLength(255)]
    public string? LogoFileName { get; set; }

    [Column("logo_content_type")]
    [StringLength(100)]
    public string? LogoContentType { get; set; }

    [Column("notes", TypeName = "text")]
    public string? Notes { get; set; }

    /// <summary>
    /// 1=AKTIF, 2=TIDAK_AKTIF, 3=BLACKLIST
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [InverseProperty("ExternalParty")]
    public virtual ICollection<MExternalPartyContact> MExternalPartyContacts { get; set; } = new List<MExternalPartyContact>();

    [InverseProperty("ExternalParty")]
    public virtual ICollection<TActivityCollaborator> TActivityCollaborators { get; set; } = new List<TActivityCollaborator>();

    [InverseProperty("ExternalParty")]
    public virtual ICollection<TBorrowRequest> TBorrowRequests { get; set; } = new List<TBorrowRequest>();

    [InverseProperty("ExternalParty")]
    public virtual ICollection<TProcurementItem> TProcurementItems { get; set; } = new List<TProcurementItem>();

    [InverseProperty("ExternalParty")]
    public virtual ICollection<TProcurementQuotation> TProcurementQuotations { get; set; } = new List<TProcurementQuotation>();

    [InverseProperty("ExternalParty")]
    public virtual ICollection<TSponsorship> TSponsorships { get; set; } = new List<TSponsorship>();
}
