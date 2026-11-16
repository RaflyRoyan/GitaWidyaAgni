using GWALib.General;
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
    /// Item anggaran (t_budget_item). Amount = Quantity x UnitPrice. Program kerja (opsional) harus dari periode anggaran yang sama.
    /// Hanya bisa diubah saat anggaran Draft / Rejected; setiap perubahan menghitung ulang total anggaran.
    /// RealizedAmount = jumlah transaksi Posted yang memakai item ini (hanya diisi oleh GetDataList).
    /// </summary>
    public partial class BudgetItem : BaseLib
    {
        public class NewRow
        {
            public int BudgetId { get; set; }
            public int CategoryId { get; set; }
            public int? ProgramId { get; set; }
            public string Name { get; set; } = null!;
            public decimal Quantity { get; set; } = 1;
            public string? Unit { get; set; }
            public decimal UnitPrice { get; set; }
        }

        public int Id { get; set; }
        public int BudgetId { get; private set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; private set; } = null!;
        public int? ProgramId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; private set; }
        public decimal RealizedAmount { get; internal set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TBudgetItem> _Querys
        {
            get
            {
                return DBClass.GetContext().TBudgetItems.Include(x => x.Category).AsQueryable();
            }
        }
        public BudgetItem()
        {
        }
        public BudgetItem(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<BudgetItem>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal BudgetItem(EF.TBudgetItem row)
        {
            SetData(row);
        }
        private void SetData(EF.TBudgetItem row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<BudgetItem>();
            BudgetId = row.BudgetId;
            CategoryId = row.CategoryId;
            CategoryName = row.Category?.Name ?? string.Empty;
            ProgramId = row.ProgramId;
            Name = row.Name;
            Quantity = row.Quantity;
            Unit = row.Unit;
            UnitPrice = row.UnitPrice;
            Amount = row.Amount;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
