using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Sponsorship
{
    /// <summary>
    /// Kesepakatan sponsorship dengan mitra (t_sponsorship). Nama kelas menghindari bentrok dengan namespace. Nomor dibuat saat simpan dari format SPONSORSHIP, atau SPN/yyyyMMdd/id bila format belum diatur.
    /// Mitra harus Aktif (blacklist ditolak). Paket opsional: nama paket disalin (PackageName), nilai default = harga paket, dan benefit paket
    /// disalin ke SponsorshipBenefit. Paket tetap setelah dibuat. Sponsorship non-dana wajib punya rincian (InKindDescription).
    /// Alur status: Aktif → Selesai (semua benefit sudah diberikan) / Batal (belum ada dana diterima). Data hanya bisa diubah saat Aktif.
    /// Dana masuk dicatat lewat ReceivePayment (transaksi Income Posted dengan sponsorship_id); ReceivedAmount = jumlah transaksi Income Posted.
    /// </summary>
    public partial class SponsorshipDeal : BaseLib
    {
        public class NewRow
        {
            public int ExternalPartyId { get; set; }
            public int? ContactId { get; set; }
            public int? PackageId { get; set; }
            // Kosong = periode kepengurusan aktif.
            public int? PeriodId { get; set; }
            public int? ActivityId { get; set; }
            public SponsorshipForm SponsorshipForm { get; set; } = SponsorshipForm.Dana;
            // 0 dengan paket = harga paket.
            public decimal Amount { get; set; }
            public string? InKindDescription { get; set; }
            public DateOnly? StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
            public int? PicMemberId { get; set; }
            public string? Notes { get; set; }
        }

        public int Id { get; set; }
        public string SponsorshipNumber { get; private set; } = null!;
        public int ExternalPartyId { get; set; }
        public string ExternalPartyName { get; private set; } = null!;
        public int? ContactId { get; set; }
        public int? PackageId { get; private set; }
        public string? PackageName { get; private set; }
        public int? PeriodId { get; set; }
        public int? ActivityId { get; set; }
        public SponsorshipForm SponsorshipForm { get; set; }
        public decimal Amount { get; set; }
        public decimal ReceivedAmount { get; internal set; }
        public string? InKindDescription { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? PicMemberId { get; set; }
        // Hanya-baca: diubah lewat Finish / Cancel.
        public SponsorshipStatus Status { get; private set; }
        public string? Notes { get; set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<SponsorshipBenefit>? BenefitList;
        private List<SponsorshipMou>? MouList;

        private static IQueryable<EF.TSponsorship> _Querys
        {
            get
            {
                return DBClass.GetContext().TSponsorships.Include(x => x.ExternalParty).AsQueryable();
            }
        }
        public SponsorshipDeal()
        {
        }
        public SponsorshipDeal(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<SponsorshipDeal>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
                ReceivedAmount = GetReceived(new() { Id }).GetValueOrDefault(Id);
            }
        }
        internal SponsorshipDeal(EF.TSponsorship row)
        {
            SetData(row);
        }
        private void SetData(EF.TSponsorship row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<SponsorshipDeal>();
            SponsorshipNumber = row.SponsorshipNumber;
            ExternalPartyId = row.ExternalPartyId;
            ExternalPartyName = row.ExternalParty?.Name ?? string.Empty;
            ContactId = row.ContactId;
            PackageId = row.PackageId;
            PackageName = row.PackageName;
            PeriodId = row.PeriodId;
            ActivityId = row.ActivityId;
            SponsorshipForm = (SponsorshipForm)row.SponsorshipForm;
            Amount = row.Amount;
            InKindDescription = row.InKindDescription;
            StartDate = row.StartDate;
            EndDate = row.EndDate;
            PicMemberId = row.PicMemberId;
            Status = (SponsorshipStatus)row.Status;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
