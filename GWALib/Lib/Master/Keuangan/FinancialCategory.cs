using GWALib.General;
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
    /// Kategori pemasukan / pengeluaran organisasi (m_financial_category). Nama unik per jenis; jenis tidak bisa diubah.
    /// </summary>
    public partial class FinancialCategory : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public CashFlowType CategoryType { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public CashFlowType CategoryType { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MFinancialCategory> _Query
        {
            get
            {
                return _context.MFinancialCategories
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MFinancialCategory> _Querys
        {
            get
            {
                return DBClass.GetContext().MFinancialCategories
                    .AsQueryable();
            }
        }
        public FinancialCategory()
        {
            _context = DBClass.GetContext();
        }
        public FinancialCategory(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<FinancialCategory>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal FinancialCategory(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal FinancialCategory(EF.MFinancialCategory row)
        {
            SetData(row);
        }
        private void SetData(EF.MFinancialCategory row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<FinancialCategory>();
            Name = row.Name;
            CategoryType = (CashFlowType)row.CategoryType;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
