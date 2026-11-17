using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.TimPelaksana;
using GWALib.Lib.Transaksi.TimPelaksana;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Kegiatan
{
    public partial class TimPelaksanaVM
    {
        /// <summary>Semua panggilan GWALib halaman Tim Pelaksana.</summary>
        public class Method
        {
            private static TimeOnly Jam(DateTime? value, string nama)
                => value.HasValue ? TimeOnly.FromDateTime(value.Value) : throw new ArgumentException(nama + " harus diisi");

            // ---------- Seksi
            public static void SimpanSeksi(NewSeksi input, string user)
                => CommitteeSection.Save(new CommitteeSection.NewRow { ActivityId = input.ActivityId, Name = input.Name ?? "", Description = input.Description, SeqNo = input.SeqNo }, user).Ok();

            public static void UpdateSeksi(string key, NewSeksi input, string user)
            {
                var obj = LibExtensions.Load<CommitteeSection>(key, "Seksi");
                obj.Name = input.Name ?? "";
                obj.Description = input.Description;
                obj.SeqNo = input.SeqNo;
                obj.Update(user).Ok();
            }

            public static void HapusSeksi(string? key, string user) => LibExtensions.Load<CommitteeSection>(key, "Seksi").Delete(user).Ok();

            // ---------- Panitia
            public static void SimpanPanitia(NewPanitia input, string user)
                => CommitteeMember.Save(new CommitteeMember.NewRow
                {
                    ActivityId = input.ActivityId,
                    MemberId = input.MemberId ?? 0,
                    SectionId = input.SectionId,
                    CommitteeRole = (CommitteeRole)input.CommitteeRole,
                    JobDesc = input.JobDesc
                }, user).Ok();

            public static void UpdatePanitia(string key, NewPanitia input, string user)
            {
                var obj = LibExtensions.Load<CommitteeMember>(key, "Panitia");
                obj.SectionId = input.SectionId;
                obj.CommitteeRole = (CommitteeRole)input.CommitteeRole;
                obj.JobDesc = input.JobDesc;
                obj.Update(user).Ok();
            }

            public static void MundurPanitia(string? key, string user) => LibExtensions.Load<CommitteeMember>(key, "Panitia").Resign(user).Ok();
            public static void KeluarkanPanitia(string? key, string user) => LibExtensions.Load<CommitteeMember>(key, "Panitia").Remove(user).Ok();

            // ---------- Rekrutmen panitia
            public static void SimpanRekrutmen(NewRekrutmen input, string user)
                => CommitteeRecruitment.Save(new CommitteeRecruitment.NewRow
                {
                    ActivityId = input.ActivityId,
                    OpenAt = input.OpenAt ?? DateTime.Now,
                    CloseAt = input.CloseAt ?? DateTime.Now,
                    Quota = input.Quota,
                    FormId = input.FormId
                }, user).Ok();

            public static void UpdateRekrutmen(string key, NewRekrutmen input, string user)
            {
                var obj = LoadRekrutmen(key);
                obj.OpenAt = input.OpenAt ?? obj.OpenAt;
                obj.CloseAt = input.CloseAt ?? obj.CloseAt;
                obj.Quota = input.Quota;
                obj.FormId = input.FormId;
                obj.Update(user).Ok();
            }

            public static CommitteeRecruitment LoadRekrutmen(string? key) => LibExtensions.Load<CommitteeRecruitment>(key, "Rekrutmen panitia");

            public static void TerimaLamaran(TerimaInput input, string user)
                => LibExtensions.Load<CommitteeApplication>(input.Key, "Lamaran").Accept(input.SectionId, input.Keterangan, user).Ok();

            public static void TolakLamaran(GwaVM.AlasanInput input, string user)
                => LibExtensions.Load<CommitteeApplication>(input.Key, "Lamaran").Reject(input.Keterangan, user).Ok();

            // ---------- Rundown
            public static void SimpanRundown(NewRundown input, string user)
                => ActivityRundown.Save(new ActivityRundown.NewRow
                {
                    ActivityId = input.ActivityId,
                    RundownDate = Opsi.TanggalWajib(input.RundownDate, "Tanggal"),
                    StartTime = Jam(input.StartTime, "Jam mulai"),
                    EndTime = input.EndTime.HasValue ? TimeOnly.FromDateTime(input.EndTime.Value) : null,
                    Title = input.Title ?? "",
                    Description = input.Description,
                    LocationName = input.LocationName,
                    SectionId = input.SectionId,
                    PicMemberId = input.PicMemberId,
                    SeqNo = input.SeqNo
                }, user).Ok();

            public static void UpdateRundown(string key, NewRundown input, string user)
            {
                var obj = LibExtensions.Load<ActivityRundown>(key, "Rundown");
                obj.RundownDate = Opsi.TanggalWajib(input.RundownDate, "Tanggal");
                obj.StartTime = Jam(input.StartTime, "Jam mulai");
                obj.EndTime = input.EndTime.HasValue ? TimeOnly.FromDateTime(input.EndTime.Value) : null;
                obj.Title = input.Title ?? "";
                obj.Description = input.Description;
                obj.LocationName = input.LocationName;
                obj.SectionId = input.SectionId;
                obj.PicMemberId = input.PicMemberId;
                obj.SeqNo = input.SeqNo;
                obj.Update(user).Ok();
            }

            public static void HapusRundown(string? key, string user) => LibExtensions.Load<ActivityRundown>(key, "Rundown").Delete(user).Ok();

            // ---------- Checklist
            public static void SimpanChecklist(NewChecklist input, string user)
                => ActivityChecklist.Save(new ActivityChecklist.NewRow
                {
                    ActivityId = input.ActivityId,
                    SectionId = input.SectionId,
                    Title = input.Title ?? "",
                    Description = input.Description,
                    DueDate = Opsi.Tanggal(input.DueDate),
                    PicMemberId = input.PicMemberId,
                    Priority = (Priority)input.Priority,
                    IsMilestone = (MilestoneFlag)input.IsMilestone
                }, user).Ok();

            public static void UpdateChecklist(string key, NewChecklist input, string user)
            {
                var obj = LibExtensions.Load<ActivityChecklist>(key, "Checklist");
                obj.SectionId = input.SectionId;
                obj.Title = input.Title ?? "";
                obj.Description = input.Description;
                obj.DueDate = Opsi.Tanggal(input.DueDate);
                obj.PicMemberId = input.PicMemberId;
                obj.Priority = (Priority)input.Priority;
                obj.IsMilestone = (MilestoneFlag)input.IsMilestone;
                obj.Update(user).Ok();
            }

            public static void StatusChecklist(GwaVM.StatusInput input, string user)
                => LibExtensions.Load<ActivityChecklist>(input.Key, "Checklist").SetStatus((TaskProgressStatus)input.Status, user).Ok();

            public static void HapusChecklist(string? key, string user) => LibExtensions.Load<ActivityChecklist>(key, "Checklist").Delete(user).Ok();

            public static int GenerateChecklist(GenerateInput input, string user)
                => ActivityChecklist.GenerateFromTemplate(input.ActivityId, input.TemplateId, user).Ok();

            // ---------- Rombongan
            public static void SimpanRombongan(NewRombongan input, byte[]? izin, IFormFile? meta, string user)
            {
                var obj = ActivityTripMember.Save(new ActivityTripMember.NewRow
                {
                    ActivityId = input.ActivityId,
                    MemberId = input.MemberId,
                    GuestName = input.MemberId.HasValue ? null : input.GuestName,
                    TripRole = (TripRole)input.TripRole,
                    Vehicle = input.Vehicle,
                    Room = input.Room,
                    EmergencyContactName = input.EmergencyContactName,
                    EmergencyContactPhone = input.EmergencyContactPhone,
                    PermissionStatus = (TripPermissionStatus)input.PermissionStatus,
                    Notes = input.Notes
                }, user).Ok();
                if (izin != null)
                {
                    obj.UpdatePermissionFile(izin, meta!.FileName, meta.ContentType, user).Ok();
                }
            }

            public static void UpdateRombongan(string key, NewRombongan input, byte[]? izin, IFormFile? meta, bool hapusIzin, string user)
            {
                var obj = LoadRombongan(key);
                if (!obj.MemberId.HasValue)
                {
                    obj.GuestName = input.GuestName;
                }
                obj.TripRole = (TripRole)input.TripRole;
                obj.Vehicle = input.Vehicle;
                obj.Room = input.Room;
                obj.EmergencyContactName = input.EmergencyContactName;
                obj.EmergencyContactPhone = input.EmergencyContactPhone;
                obj.PermissionStatus = (TripPermissionStatus)input.PermissionStatus;
                obj.Notes = input.Notes;
                obj.Update(user).Ok();
                if (izin != null)
                {
                    obj.UpdatePermissionFile(izin, meta!.FileName, meta.ContentType, user).Ok();
                }
                else if (hapusIzin)
                {
                    obj.UpdatePermissionFile(null, null, null, user).Ok();
                }
            }

            public static ActivityTripMember LoadRombongan(string? key) => LibExtensions.Load<ActivityTripMember>(key, "Rombongan");

            // ---------- Template checklist
            public static void SimpanTemplate(NewTemplate input, string user)
                => ChecklistTemplate.Save(new ChecklistTemplate.NewRow
                {
                    Name = input.Name ?? "",
                    ActivityType = input.ActivityType.HasValue ? (ActivityType)input.ActivityType.Value : null,
                    Description = input.Description
                }, user).Ok();

            public static void UpdateTemplate(string key, NewTemplate input, string user)
            {
                var obj = LibExtensions.Load<ChecklistTemplate>(key, "Template");
                obj.Name = input.Name ?? "";
                obj.ActivityType = input.ActivityType.HasValue ? (ActivityType)input.ActivityType.Value : null;
                obj.Description = input.Description;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }

            public static void HapusTemplate(string? key, string user) => LibExtensions.Load<ChecklistTemplate>(key, "Template").Delete(user).Ok();

            public static void TambahItem(NewItem input, string user)
            {
                var obj = LibExtensions.Load<ChecklistTemplate>(input.Key, "Template");
                ChecklistTemplateItem.Save(new ChecklistTemplateItem.NewRow
                {
                    TemplateId = obj.Id,
                    Title = input.Title ?? "",
                    SectionName = input.SectionName,
                    OffsetDay = input.OffsetDay,
                    Priority = (Priority)input.Priority,
                    IsMilestone = (MilestoneFlag)input.IsMilestone
                }, user).Ok();
            }

            public static void HapusItem(string? key, string user) => LibExtensions.Load<ChecklistTemplateItem>(key, "Item template").Delete(user).Ok();
        }
    }
}
