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
    /// Item berita acara serah terima (t_period_handover_item): saldo, aset, program, hutang, dokumen, sponsor berjalan, dll.
    /// RefId (opsional) menunjuk data sesuai ItemType (lihat PeriodHandoverItem.ValidateRef). Hanya bisa diubah saat berita acara Draft / Ditolak.
    /// </summary>
    public partial class PeriodHandoverItem : BaseLib
    {
        public class NewRow
        {
            public int HandoverId { get; set; }
            public HandoverItemType ItemType { get; set; }
            public int? RefId { get; set; }
            public string Description { get; set; } = null!;
            public decimal? Amount { get; set; }
            public int? Quantity { get; set; }
            public ItemCondition? ItemCondition { get; set; }
            public string? Notes { get; set; }
        }

        public int Id { get; set; }
        public int HandoverId { get; private set; }
        public HandoverItemType ItemType { get; set; }
        public int? RefId { get; set; }
        public string Description { get; set; } = null!;
        public decimal? Amount { get; set; }
        public int? Quantity { get; set; }
        public ItemCondition? ItemCondition { get; set; }
        public string? Notes { get; set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TPeriodHandoverItem> _Querys
        {
            get
            {
                return DBClass.GetContext().TPeriodHandoverItems.AsQueryable();
            }
        }
        public PeriodHandoverItem()
        {
        }
        public PeriodHandoverItem(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<PeriodHandoverItem>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal PeriodHandoverItem(EF.TPeriodHandoverItem row)
        {
            SetData(row);
        }
        private void SetData(EF.TPeriodHandoverItem row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<PeriodHandoverItem>();
            HandoverId = row.HandoverId;
            ItemType = (HandoverItemType)row.ItemType;
            RefId = row.RefId;
            Description = row.Description;
            Amount = row.Amount;
            Quantity = row.Quantity;
            ItemCondition = row.ItemCondition != null ? (ItemCondition)row.ItemCondition : null;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
