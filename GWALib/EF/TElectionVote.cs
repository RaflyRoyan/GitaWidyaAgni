using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_election_vote")]
[Index("ElectionId", Name = "ix_vote_election")]
[Index("BallotReference", Name = "uq_ballot_reference", IsUnique = true)]
public partial class TElectionVote
{
    /// <summary>
    /// ACAK (bukan dari m_sequence) agar urutan memilih tidak terbaca
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("election_id")]
    public int ElectionId { get; set; }

    /// <summary>
    /// Hash acak untuk verifikasi pemilih tanpa membuka identitas
    /// </summary>
    [Column("ballot_reference")]
    [StringLength(64)]
    public string BallotReference { get; set; } = null!;

    /// <summary>
    /// AES-256-GCM: id kandidat + nilai acak. Didekripsi hanya saat finalisasi
    /// </summary>
    [Column("encrypted_choice", TypeName = "text")]
    public string EncryptedChoice { get; set; } = null!;

    /// <summary>
    /// Isi tanggal saja (jam 00:00), jangan waktu persis
    /// </summary>
    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    /// <summary>
    /// Selalu NULL, jangan isi id pemilih
    /// </summary>
    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ElectionId")]
    [InverseProperty("TElectionVotes")]
    public virtual TElection Election { get; set; } = null!;
}
