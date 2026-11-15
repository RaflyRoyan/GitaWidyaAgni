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
    /// Akun kas / bank / e-wallet organisasi (m_financial_account). Nama unik.
    /// Saldo awal dikunci setelah akun punya transaksi atau saldo periode.
    /// </summary>
    public partial class FinancialAccount : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public FinancialAccountType AccountType { get; set; } = FinancialAccountType.Cash;
            // Bank: nama bank; e-wallet: nama penyedia (opsional).
            public string? BankName { get; set; }
            public string? AccountNumber { get; set; }
            public string? AccountHolder { get; set; }
            public decimal OpeningBalance { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public FinancialAccountType AccountType { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolder { get; set; }
        public decimal OpeningBalance { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MFinancialAccount> _Query
        {
            get
            {
                return _context.MFinancialAccounts
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MFinancialAccount> _Querys
        {
            get
            {
                return DBClass.GetContext().MFinancialAccounts
                    .AsQueryable();
            }
        }
        public FinancialAccount()
        {
            _context = DBClass.GetContext();
        }
        public FinancialAccount(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<FinancialAccount>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal FinancialAccount(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal FinancialAccount(EF.MFinancialAccount row)
        {
            SetData(row);
        }
        private void SetData(EF.MFinancialAccount row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<FinancialAccount>();
            Name = row.Name;
            AccountType = (FinancialAccountType)row.AccountType;
            BankName = row.BankName;
            AccountNumber = row.AccountNumber;
            AccountHolder = row.AccountHolder;
            OpeningBalance = row.OpeningBalance;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
