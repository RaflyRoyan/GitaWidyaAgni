using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Anggota
{
    /// <summary>
    /// Berita acara serah terima kepengurusan (t_period_handover), satu per periode asal.
    /// Alur status: Draft → Diajukan → Disetujui / Ditolak; Ditolak bisa diperbaiki lalu diajukan lagi. Ubah data / item / LPJ hanya saat Draft / Ditolak.
    /// Disetujui: periode asal ditutup, periode tujuan diaktifkan, penugasan pengurus periode asal diakhiri, dan nomor berita acara
    /// dibuat dari format BA_ST bila belum diisi dan formatnya ada. LPJ tidak ikut dimuat (GetLpj / UpdateLpj).
    /// </summary>
    public partial class PeriodHandover : BaseLib
    {
        public class NewRow
        {
            public int FromPeriodId { get; set; }
            public int ToPeriodId { get; set; }
            // Kosong = dibuat otomatis saat disetujui (format BA_ST).
            public string? HandoverNumber { get; set; }
            public DateOnly HandoverDate { get; set; }
            public int? HandedByMemberId { get; set; }
            public int? ReceivedByMemberId { get; set; }
            public string? Notes { get; set; }
            // Opsional: item yang langsung dibuat bersama berita acara (HandoverId diisi otomatis).
            public List<PeriodHandoverItem.NewRow>? Items { get; set; }
        }
        public record FileContent(byte[] Data, string FileName);

        public int Id { get; set; }
        public int FromPeriodId { get; private set; }
        public string FromPeriodName { get; private set; } = null!;
        public int ToPeriodId { get; set; }
        public string ToPeriodName { get; private set; } = null!;
        public string? HandoverNumber { get; set; }
        public DateOnly HandoverDate { get; set; }
        public int? HandedByMemberId { get; set; }
        public int? ReceivedByMemberId { get; set; }
        public string? Notes { get; set; }
        // Terisi = file LPJ ada.
        public string? LpjFileName { get; private set; }
        // Hanya-baca: diubah lewat Submit / Approve / Reject.
        public HandoverStatus Status { get; private set; }
        public int? ApprovedBy { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<PeriodHandoverItem>? ItemList;

        // Kolom LPJ (blob) tidak ikut di-select.
        private static IQueryable<EF.TPeriodHandover> _Querys
        {
            get
            {
                return DBClass.GetContext().TPeriodHandovers.Select(x => new EF.TPeriodHandover
                {
                    Id = x.Id,
                    FromPeriodId = x.FromPeriodId,
                    FromPeriod = x.FromPeriod,
                    ToPeriodId = x.ToPeriodId,
                    ToPeriod = x.ToPeriod,
                    HandoverNumber = x.HandoverNumber,
                    HandoverDate = x.HandoverDate,
                    HandedByMemberId = x.HandedByMemberId,
                    ReceivedByMemberId = x.ReceivedByMemberId,
                    Notes = x.Notes,
                    LpjFileName = x.LpjFileName,
                    Status = x.Status,
                    ApprovedBy = x.ApprovedBy,
                    ApprovedAt = x.ApprovedAt,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                });
            }
        }
        public PeriodHandover()
        {
        }
        public PeriodHandover(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<PeriodHandover>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal PeriodHandover(EF.TPeriodHandover row)
        {
            SetData(row);
        }
        private void SetData(EF.TPeriodHandover row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<PeriodHandover>();
            FromPeriodId = row.FromPeriodId;
            FromPeriodName = row.FromPeriod?.Name ?? string.Empty;
            ToPeriodId = row.ToPeriodId;
            ToPeriodName = row.ToPeriod?.Name ?? string.Empty;
            HandoverNumber = row.HandoverNumber;
            HandoverDate = row.HandoverDate;
            HandedByMemberId = row.HandedByMemberId;
            ReceivedByMemberId = row.ReceivedByMemberId;
            Notes = row.Notes;
            LpjFileName = row.LpjFileName;
            Status = (HandoverStatus)row.Status;
            ApprovedBy = row.ApprovedBy;
            ApprovedAt = row.ApprovedAt;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
