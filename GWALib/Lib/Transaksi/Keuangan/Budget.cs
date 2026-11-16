using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Keuangan
{
    /// <summary>
    /// Rencana anggaran per periode kepengurusan (t_budget). Alur status: Draft → Submitted → Approved / Rejected; Rejected bisa diperbaiki lalu diajukan lagi.
    /// Nama dan item hanya bisa diubah saat Draft / Rejected. TotalAmount = jumlah amount item, dihitung ulang setiap item berubah.
    /// Item anggaran baru bisa dipakai transaksi / pengajuan dana setelah anggaran Approved.
    /// </summary>
    public partial class Budget : BaseLib
    {
        public class NewRow
        {
            public int PeriodId { get; set; }
            public string Name { get; set; } = null!;
            // Opsional: item yang langsung dibuat bersama anggaran (BudgetId diisi otomatis).
            public List<BudgetItem.NewRow>? Items { get; set; }
        }

        public int Id { get; set; }
        public int PeriodId { get; private set; }
        public string PeriodName { get; private set; } = null!;
        public string Name { get; set; } = null!;
        public decimal TotalAmount { get; private set; }
        // Hanya-baca: diubah lewat Submit / Approve / Reject.
        public BudgetStatus Status { get; private set; }
        public int? ApprovedBy { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<BudgetItem>? ItemList;
        private List<BudgetDocument>? DocumentList;

        private static IQueryable<EF.TBudget> _Querys
        {
            get
            {
                return DBClass.GetContext().TBudgets.Include(x => x.Period).AsQueryable();
            }
        }
        public Budget()
        {
        }
        public Budget(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Budget>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Budget(EF.TBudget row)
        {
            SetData(row);
        }
        private void SetData(EF.TBudget row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Budget>();
            PeriodId = row.PeriodId;
            PeriodName = row.Period?.Name ?? string.Empty;
            Name = row.Name;
            TotalAmount = row.TotalAmount;
            Status = (BudgetStatus)row.Status;
            ApprovedBy = row.ApprovedBy;
            ApprovedAt = row.ApprovedAt;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
