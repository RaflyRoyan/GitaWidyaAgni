using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.TimPelaksana
{
    /// <summary>
    /// Manifest rombongan kegiatan luar kota (t_activity_trip_member): peserta, official, pelatih, kru.
    /// MemberId (anggota, sekali per kegiatan) atau GuestName (non-anggota, mis. sopir) wajib salah satu.
    /// File izin orang tua / instansi tidak ikut dimuat (GetPermissionFile / UpdatePermissionFile); mengunggah file = status izin Ada.
    /// </summary>
    public partial class ActivityTripMember : BaseLib
    {
        public class NewRow
        {
            public int ActivityId { get; set; }
            public int? MemberId { get; set; }
            // Wajib bila MemberId kosong.
            public string? GuestName { get; set; }
            public TripRole TripRole { get; set; } = TripRole.Peserta;
            public string? Vehicle { get; set; }
            public string? Room { get; set; }
            public string? EmergencyContactName { get; set; }
            public string? EmergencyContactPhone { get; set; }
            public TripPermissionStatus PermissionStatus { get; set; } = TripPermissionStatus.Belum;
            public string? Notes { get; set; }
        }
        public record FileContent(byte[] Data, string FileName, string ContentType);

        public int Id { get; set; }
        public int ActivityId { get; private set; }
        public int? MemberId { get; private set; }
        // Nama anggota, atau GuestName untuk non-anggota.
        public string DisplayName { get; private set; } = null!;
        public string? GuestName { get; set; }
        public TripRole TripRole { get; set; }
        public string? Vehicle { get; set; }
        public string? Room { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public TripPermissionStatus PermissionStatus { get; set; }
        // Terisi = file izin ada.
        public string? PermissionFileName { get; private set; }
        public string? Notes { get; set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        // Kolom file izin (blob) tidak ikut di-select.
        private static IQueryable<EF.TActivityTripMember> _Querys
        {
            get
            {
                return DBClass.GetContext().TActivityTripMembers.Select(x => new EF.TActivityTripMember
                {
                    Id = x.Id,
                    ActivityId = x.ActivityId,
                    MemberId = x.MemberId,
                    Member = x.Member == null ? null : new EF.MMember { Id = x.Member.Id, Fullname = x.Member.Fullname },
                    GuestName = x.GuestName,
                    TripRole = x.TripRole,
                    Vehicle = x.Vehicle,
                    Room = x.Room,
                    EmergencyContactName = x.EmergencyContactName,
                    EmergencyContactPhone = x.EmergencyContactPhone,
                    PermissionStatus = x.PermissionStatus,
                    PermissionFileName = x.PermissionFileName,
                    PermissionContentType = x.PermissionContentType,
                    Notes = x.Notes,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                });
            }
        }
        public ActivityTripMember()
        {
        }
        public ActivityTripMember(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ActivityTripMember>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ActivityTripMember(EF.TActivityTripMember row)
        {
            SetData(row);
        }
        private void SetData(EF.TActivityTripMember row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ActivityTripMember>();
            ActivityId = row.ActivityId;
            MemberId = row.MemberId;
            DisplayName = row.Member?.Fullname ?? row.GuestName ?? string.Empty;
            GuestName = row.GuestName;
            TripRole = (TripRole)row.TripRole;
            Vehicle = row.Vehicle;
            Room = row.Room;
            EmergencyContactName = row.EmergencyContactName;
            EmergencyContactPhone = row.EmergencyContactPhone;
            PermissionStatus = (TripPermissionStatus)row.PermissionStatus;
            PermissionFileName = row.PermissionFileName;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
