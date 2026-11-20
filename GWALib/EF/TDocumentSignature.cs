using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Tanda tangan elektronik (QR) pada laporan cetak. Ditulis tangan (migrasi 2026-11-20_document_signature.sql); tanpa navigasi ke m_user.
/// </summary>
[Table("t_document_signature")]
[Index("Token", Name = "uq_document_signature_token", IsUnique = true)]
[Index("Fingerprint", Name = "uq_document_signature_fingerprint", IsUnique = true)]
[Index("InsBy", Name = "fk_document_signature_ins_by")]
[Index("LastPrintedBy", Name = "fk_document_signature_printed_by")]
public partial class TDocumentSignature
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>Token acak 128-bit (hex huruf besar) untuk QR /ttd/{token}</summary>
    [Column("token")]
    [StringLength(32)]
    public string Token { get; set; } = null!;

    /// <summary>SHA-256 hex dari jenis|judul|referensi|jabatan|nama|tanggal</summary>
    [Column("fingerprint")]
    [StringLength(64)]
    public string Fingerprint { get; set; } = null!;

    [Column("doc_type")]
    [StringLength(40)]
    public string DocType { get; set; } = null!;

    [Column("doc_title")]
    [StringLength(200)]
    public string DocTitle { get; set; } = null!;

    [Column("doc_ref")]
    [StringLength(255)]
    public string? DocRef { get; set; }

    [Column("signer_role")]
    [StringLength(100)]
    public string SignerRole { get; set; } = null!;

    [Column("signer_name")]
    [StringLength(150)]
    public string SignerName { get; set; } = null!;

    [Column("signer_number")]
    [StringLength(50)]
    public string? SignerNumber { get; set; }

    [Column("sign_date")]
    public DateOnly SignDate { get; set; }

    [Column("print_count")]
    public int PrintCount { get; set; }

    [Column("last_printed_at", TypeName = "datetime")]
    public DateTime LastPrintedAt { get; set; }

    [Column("last_printed_by")]
    public int? LastPrintedBy { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }
}
