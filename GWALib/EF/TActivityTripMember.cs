using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_activity_trip_member")]
[Index("MemberId", Name = "fk_trip_member")]
[Index("ActivityId", "MemberId", Name = "uq_trip_member", IsUnique = true)]
public partial class TActivityTripMember
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("activity_id")]
    public int ActivityId { get; set; }

    /// <summary>
    /// NULL untuk non-anggota (pelatih eksternal, sopir)
    /// </summary>
    [Column("member_id")]
    public int? MemberId { get; set; }

    /// <summary>
    /// Wajib jika member_id NULL
    /// </summary>
    [Column("guest_name")]
    [StringLength(150)]
    public string? GuestName { get; set; }

    /// <summary>
    /// 1=PESERTA, 2=OFFICIAL, 3=PELATIH, 4=KRU, 5=LAINNYA
    /// </summary>
    [Column("trip_role")]
    public int TripRole { get; set; }

    [Column("vehicle")]
    [StringLength(100)]
    public string? Vehicle { get; set; }

    [Column("room")]
    [StringLength(50)]
    public string? Room { get; set; }

    [Column("emergency_contact_name")]
    [StringLength(150)]
    public string? EmergencyContactName { get; set; }

    [Column("emergency_contact_phone")]
    [StringLength(30)]
    public string? EmergencyContactPhone { get; set; }

    /// <summary>
    /// 1=BELUM, 2=ADA, 3=TIDAK_PERLU (izin orang tua/instansi)
    /// </summary>
    [Column("permission_status")]
    public int PermissionStatus { get; set; }

    [Column("permission_file", TypeName = "mediumblob")]
    public byte[]? PermissionFile { get; set; }

    [Column("permission_file_name")]
    [StringLength(255)]
    public string? PermissionFileName { get; set; }

    [Column("permission_content_type")]
    [StringLength(100)]
    public string? PermissionContentType { get; set; }

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

    [ForeignKey("ActivityId")]
    [InverseProperty("TActivityTripMembers")]
    public virtual TActivity Activity { get; set; } = null!;

    [ForeignKey("MemberId")]
    [InverseProperty("TActivityTripMembers")]
    public virtual MMember? Member { get; set; }
}
