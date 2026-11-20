using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_member")]
[Index("Nim", Name = "uq_member_nim", IsUnique = true)]
[Index("MemberNumber", Name = "uq_member_number", IsUnique = true)]
[Index("UserId", Name = "uq_member_user", IsUnique = true)]
[Index("CardToken", Name = "uq_member_card_token", IsUnique = true)]
public partial class MMember
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// NULL jika belum punya akun login
    /// </summary>
    [Column("user_id")]
    public int? UserId { get; set; }

    /// <summary>
    /// Nomor Induk Anggota (NIA)
    /// </summary>
    [Column("member_number")]
    [StringLength(30)]
    public string? MemberNumber { get; set; }

    /// <summary>
    /// 1=ANGGOTA, 2=PENGURUS, 4=ALUMNI (3 tidak dipakai)
    /// </summary>
    [Column("member_type")]
    public int MemberType { get; set; }

    /// <summary>
    /// Tanggal bergabung
    /// </summary>
    [Column("joined_at")]
    public DateOnly? JoinedAt { get; set; }

    /// <summary>
    /// Tanggal keluar
    /// </summary>
    [Column("left_at")]
    public DateOnly? LeftAt { get; set; }

    /// <summary>
    /// 1=MAHASISWA, 2=UMUM
    /// </summary>
    [Column("member_category")]
    public int MemberCategory { get; set; }

    /// <summary>
    /// Wajib untuk MAHASISWA
    /// </summary>
    [Column("nim")]
    [StringLength(30)]
    public string? Nim { get; set; }

    [Column("fullname")]
    [StringLength(150)]
    public string Fullname { get; set; } = null!;

    /// <summary>
    /// 1=L, 2=P
    /// </summary>
    [Column("gender")]
    public int? Gender { get; set; }

    [Column("birth_date")]
    public DateOnly? BirthDate { get; set; }

    [Column("fakultas")]
    [StringLength(100)]
    public string? Fakultas { get; set; }

    [Column("program_studi")]
    [StringLength(100)]
    public string? ProgramStudi { get; set; }

    [Column("angkatan")]
    public short? Angkatan { get; set; }

    /// <summary>
    /// Asal instansi / kampus lain / pekerjaan (untuk UMUM)
    /// </summary>
    [Column("institution")]
    [StringLength(150)]
    public string? Institution { get; set; }

    [Column("phone")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [Column("email")]
    [StringLength(150)]
    public string? Email { get; set; }

    [Column("address")]
    [StringLength(255)]
    public string? Address { get; set; }

    /// <summary>
    /// Token acak 128-bit (hex huruf besar) untuk QR KTA /kta/{token}; NULL = KTA belum pernah diterbitkan
    /// </summary>
    [Column("card_token")]
    [StringLength(32)]
    public string? CardToken { get; set; }

    /// <summary>
    /// Waktu token KTA dibuat / diganti
    /// </summary>
    [Column("card_issued_at", TypeName = "datetime")]
    public DateTime? CardIssuedAt { get; set; }

    /// <summary>
    /// 1=ACTIVE, 2=ALUMNI, 3=INACTIVE
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

    [InverseProperty("Member")]
    public virtual MAlumniProfile? MAlumniProfile { get; set; }

    [InverseProperty("Member")]
    public virtual ICollection<MMemberPhoto> MMemberPhotos { get; set; } = new List<MMemberPhoto>();

    [InverseProperty("Member")]
    public virtual ICollection<TAchievement> TAchievements { get; set; } = new List<TAchievement>();

    [InverseProperty("PicMember")]
    public virtual ICollection<TActivityChecklist> TActivityChecklists { get; set; } = new List<TActivityChecklist>();

    [InverseProperty("Member")]
    public virtual ICollection<TActivityParticipant> TActivityParticipants { get; set; } = new List<TActivityParticipant>();

    [InverseProperty("PicMember")]
    public virtual ICollection<TActivityRundown> TActivityRundowns { get; set; } = new List<TActivityRundown>();

    [InverseProperty("Member")]
    public virtual ICollection<TActivityTripMember> TActivityTripMembers { get; set; } = new List<TActivityTripMember>();

    [InverseProperty("Member")]
    public virtual ICollection<TAttendanceRecord> TAttendanceRecords { get; set; } = new List<TAttendanceRecord>();

    [InverseProperty("Member")]
    public virtual ICollection<TBorrowRequest> TBorrowRequests { get; set; } = new List<TBorrowRequest>();

    [InverseProperty("Member")]
    public virtual ICollection<TCertificate> TCertificates { get; set; } = new List<TCertificate>();

    [InverseProperty("Member")]
    public virtual ICollection<TCommitteeApplication> TCommitteeApplications { get; set; } = new List<TCommitteeApplication>();

    [InverseProperty("Member")]
    public virtual ICollection<TCommitteeMember> TCommitteeMembers { get; set; } = new List<TCommitteeMember>();

    [InverseProperty("CoordinatorMember")]
    public virtual ICollection<TCommitteeSection> TCommitteeSections { get; set; } = new List<TCommitteeSection>();

    [InverseProperty("Member")]
    public virtual ICollection<TCompetitionParticipant> TCompetitionParticipants { get; set; } = new List<TCompetitionParticipant>();

    [InverseProperty("Member")]
    public virtual ICollection<TDivisionMember> TDivisionMembers { get; set; } = new List<TDivisionMember>();

    [InverseProperty("Member")]
    public virtual ICollection<TSectionMember> TSectionMembers { get; set; } = new List<TSectionMember>();

    [InverseProperty("Member")]
    public virtual ICollection<TDuesBill> TDuesBills { get; set; } = new List<TDuesBill>();

    [InverseProperty("Member")]
    public virtual ICollection<TElectionCandidate> TElectionCandidates { get; set; } = new List<TElectionCandidate>();

    [InverseProperty("Member")]
    public virtual ICollection<TElectionEligibleVoter> TElectionEligibleVoters { get; set; } = new List<TElectionEligibleVoter>();

    [InverseProperty("Member")]
    public virtual ICollection<TFormResponse> TFormResponses { get; set; } = new List<TFormResponse>();

    [InverseProperty("ToMember")]
    public virtual ICollection<TLetterDisposition> TLetterDispositions { get; set; } = new List<TLetterDisposition>();

    [InverseProperty("Member")]
    public virtual ICollection<TManagementAssignment> TManagementAssignments { get; set; } = new List<TManagementAssignment>();

    [InverseProperty("ChairedByMember")]
    public virtual ICollection<TMeetingMinute> TMeetingMinuteChairedByMembers { get; set; } = new List<TMeetingMinute>();

    [InverseProperty("NotulisMember")]
    public virtual ICollection<TMeetingMinute> TMeetingMinuteNotulisMembers { get; set; } = new List<TMeetingMinute>();

    [InverseProperty("Member")]
    public virtual ICollection<TMemberPoint> TMemberPoints { get; set; } = new List<TMemberPoint>();

    [InverseProperty("HandedByMember")]
    public virtual ICollection<TPeriodHandover> TPeriodHandoverHandedByMembers { get; set; } = new List<TPeriodHandover>();

    [InverseProperty("ReceivedByMember")]
    public virtual ICollection<TPeriodHandover> TPeriodHandoverReceivedByMembers { get; set; } = new List<TPeriodHandover>();

    [InverseProperty("ReceivedByMember")]
    public virtual ICollection<TProcurementReceipt> TProcurementReceipts { get; set; } = new List<TProcurementReceipt>();

    [InverseProperty("RequestedByMember")]
    public virtual ICollection<TProcurementRequest> TProcurementRequests { get; set; } = new List<TProcurementRequest>();

    [InverseProperty("ResponsibleMember")]
    public virtual ICollection<TProgramTask> TProgramTasks { get; set; } = new List<TProgramTask>();

    [InverseProperty("PicMember")]
    public virtual ICollection<TProgram> TPrograms { get; set; } = new List<TProgram>();

    [InverseProperty("Member")]
    public virtual ICollection<TRecruitmentApplicant> TRecruitmentApplicants { get; set; } = new List<TRecruitmentApplicant>();

    [InverseProperty("BuyerMember")]
    public virtual ICollection<TSalesOrder> TSalesOrderBuyerMembers { get; set; } = new List<TSalesOrder>();

    [InverseProperty("PicMember")]
    public virtual ICollection<TSalesOrder> TSalesOrderPicMembers { get; set; } = new List<TSalesOrder>();

    [InverseProperty("PicMember")]
    public virtual ICollection<TSponsorship> TSponsorships { get; set; } = new List<TSponsorship>();

    [InverseProperty("PicMember")]
    public virtual ICollection<TTimelinePlan> TTimelinePlans { get; set; } = new List<TTimelinePlan>();

    [ForeignKey("UserId")]
    [InverseProperty("MMember")]
    public virtual MUser? User { get; set; }
}
