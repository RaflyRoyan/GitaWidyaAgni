using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_competition")]
public partial class TCompetition
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Penyelenggara
    /// </summary>
    [Column("organizer")]
    [StringLength(200)]
    public string? Organizer { get; set; }

    /// <summary>
    /// 1=INTERNAL, 2=KAMPUS, 3=REGIONAL, 4=NASIONAL, 5=INTERNASIONAL
    /// </summary>
    [Column("level")]
    public int Level { get; set; }

    [Column("category")]
    [StringLength(100)]
    public string? Category { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("registration_deadline", TypeName = "datetime")]
    public DateTime? RegistrationDeadline { get; set; }

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("location")]
    [StringLength(255)]
    public string? Location { get; set; }

    [Column("poster", TypeName = "mediumblob")]
    public byte[]? Poster { get; set; }

    [Column("link_url")]
    [StringLength(255)]
    public string? LinkUrl { get; set; }

    /// <summary>
    /// 0=Private, 1=Public
    /// </summary>
    [Column("is_public")]
    public int IsPublic { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | disorot di landing page
    /// </summary>
    [Column("is_featured")]
    public int IsFeatured { get; set; }

    /// <summary>
    /// 1=UPCOMING, 2=REGISTRATION, 3=ONGOING, 4=FINISHED, 5=CANCELLED
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

    [InverseProperty("Competition")]
    public virtual ICollection<TAchievement> TAchievements { get; set; } = new List<TAchievement>();

    [InverseProperty("Competition")]
    public virtual ICollection<TActivity> TActivities { get; set; } = new List<TActivity>();

    [InverseProperty("Competition")]
    public virtual ICollection<TCompetitionMaterial> TCompetitionMaterials { get; set; } = new List<TCompetitionMaterial>();

    [InverseProperty("Competition")]
    public virtual ICollection<TCompetitionParticipant> TCompetitionParticipants { get; set; } = new List<TCompetitionParticipant>();

    [InverseProperty("Competition")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactions { get; set; } = new List<TFinancialTransaction>();

    [InverseProperty("Competition")]
    public virtual ICollection<TFundingRequest> TFundingRequests { get; set; } = new List<TFundingRequest>();
}
