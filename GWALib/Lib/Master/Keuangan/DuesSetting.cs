using GWALib.General;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Keuangan
{
    /// <summary>
    /// Pengaturan iuran organisasi (m_dues_setting), mis. Iuran Bulanan 2026. Periode dan kategori pemasukan opsional.
    /// Tagihan (t_dues_bill) menyimpan nominalnya sendiri, jadi nominal boleh diubah; periode dan frekuensi dikunci setelah ada tagihan.
    /// </summary>
    public partial class DuesSetting : BaseLib
    {
        public class NewRow
        {
            public int? PeriodId { get; set; }
            public string Name { get; set; } = null!;
            public decimal Amount { get; set; }
            public DuesFrequency Frequency { get; set; } = DuesFrequency.Monthly;
            // Kategori pemasukan (Income) saat iuran dibayar.
            public int? CategoryId { get; set; }
        }

        public int Id { get; set; }
        public ManagementPeriod? Period { get; set; }
        public string Name { get; set; } = null!;
        public decimal Amount { get; set; }
        public DuesFrequency Frequency { get; set; }
        public FinancialCategory? Category { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }
        EF.ModelContext _context = null!;
        private IQueryable<EF.MDuesSetting> _Query
        {
            get
            {
                return _context.MDuesSettings
                    .Include(x => x.Period)
                    .Include(x => x.Category)
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MDuesSetting> _Querys
        {
            get
            {
                return DBClass.GetContext().MDuesSettings
                    .Include(x => x.Period)
                    .Include(x => x.Category)
                    .AsQueryable();
            }
        }
        public DuesSetting()
        {
            _context = DBClass.GetContext();
        }
        public DuesSetting(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<DuesSetting>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal DuesSetting(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal DuesSetting(EF.MDuesSetting row)
        {
            SetData(row);
        }
        private void SetData(EF.MDuesSetting row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<DuesSetting>();
            Period = row.Period != null ? new ManagementPeriod(row.Period) : row.PeriodId != null ? new ManagementPeriod(row.PeriodId.Value) : null;
            Name = row.Name;
            Amount = row.Amount;
            Frequency = (DuesFrequency)row.Frequency;
            Category = row.Category != null ? new FinancialCategory(row.Category) : row.CategoryId != null ? new FinancialCategory(row.CategoryId.Value) : null;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
