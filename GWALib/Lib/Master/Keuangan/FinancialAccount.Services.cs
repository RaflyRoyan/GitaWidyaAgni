using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Keuangan
{
    public partial class FinancialAccount
    {
        #region SYNC
        public static List<FinancialAccount> GetDataList(FilterAktif aktif, FinancialAccountType? accountType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, accountType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<FinancialAccount, EF.MFinancialAccount>();
        }
        public static Response<FinancialAccount> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MFinancialAccounts.Add(row);
                context.SaveChanges();
                return new FinancialAccount(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FinancialAccount>(msg));
        }
        public Response<FinancialAccount> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new FinancialAccount(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FinancialAccount>(msg));
        }
        public Response<FinancialAccount> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new FinancialAccount(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FinancialAccount>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<FinancialAccount>> GetDataListAsync(FilterAktif aktif, FinancialAccountType? accountType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, accountType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<FinancialAccount, EF.MFinancialAccount>();
        }
        public static async Task<Response<FinancialAccount>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MFinancialAccounts.Add(row);
                await context.SaveChangesAsync();
                return new FinancialAccount(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FinancialAccount>(msg));
        }
        public async Task<Response<FinancialAccount>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new FinancialAccount(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FinancialAccount>(msg));
        }
        public async Task<Response<FinancialAccount>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new FinancialAccount(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FinancialAccount>(msg));
        }
        #endregion

        #region HELPER
        internal static void ValidateActiveAccount(EF.ModelContext context, int accountId)
        {
            var row = context.MFinancialAccounts.SingleOrDefault(x => x.Id == accountId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Akun Keuangan"));
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Akun Keuangan"));
            }
        }
        private static IQueryable<EF.MFinancialAccount> FilterQuery(FilterAktif aktif, FinancialAccountType? accountType)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (accountType == null || x.AccountType == (int)accountType))
                .OrderBy(x => x.AccountType).ThenBy(x => x.Name);
        }
        private static EF.MFinancialAccount GetRow(EF.ModelContext context, int id)
        {
            return context.MFinancialAccounts.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Akun Keuangan"));
        }
        private static async Task<EF.MFinancialAccount> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MFinancialAccounts.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Akun Keuangan"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        // Bank wajib nama bank + nomor rekening; e-wallet wajib nomor akun; kas tidak memakai kolom rekening.
        private static void ValidateData(EF.ModelContext context, int id, string name, FinancialAccountType accountType,
            string? bankName, string? accountNumber, decimal openingBalance)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Akun"));
            }
            if (!Enum.IsDefined(accountType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Akun"));
            }
            if (accountType == FinancialAccountType.Bank && bankName == null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Bank"));
            }
            if (accountType != FinancialAccountType.Cash && accountNumber == null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nomor Rekening / Akun"));
            }
            if (openingBalance < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Saldo Awal"));
            }
            ValidateUtility.CheckDuplicateData<EF.MFinancialAccount>(context, x => x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Akun"));
        }
        private static void ValidateOpeningBalanceChange(EF.ModelContext context, EF.MFinancialAccount row)
        {
            if (context.TFinancialTransactions.Any(x => x.AccountId == row.Id) || context.TPeriodBalances.Any(x => x.AccountId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.FINANCIAL_ACCOUNT_BALANCE_LOCKED);
            }
        }
        private static EF.MFinancialAccount CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            var isCash = newrow.AccountType == FinancialAccountType.Cash;
            var bankName = isCash ? null : ToNullable(newrow.BankName);
            var accountNumber = isCash ? null : ToNullable(newrow.AccountNumber);
            ValidateData(context, 0, name, newrow.AccountType, bankName, accountNumber, newrow.OpeningBalance);

            var row = new EF.MFinancialAccount
            {
                Id = SequenceUtility.GetNextId<EF.MFinancialAccount>(context),
                Name = name,
                AccountType = (int)newrow.AccountType,
                BankName = bankName,
                AccountNumber = accountNumber,
                AccountHolder = isCash ? null : ToNullable(newrow.AccountHolder),
                OpeningBalance = newrow.OpeningBalance,
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MFinancialAccount row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            var isCash = AccountType == FinancialAccountType.Cash;
            var bankName = isCash ? null : ToNullable(BankName);
            var accountNumber = isCash ? null : ToNullable(AccountNumber);
            ValidateData(context, row.Id, name, AccountType, bankName, accountNumber, OpeningBalance);
            if (OpeningBalance != row.OpeningBalance)
            {
                ValidateOpeningBalanceChange(context, row);
            }

            row.Name = name;
            row.AccountType = (int)AccountType;
            row.BankName = bankName;
            row.AccountNumber = accountNumber;
            row.AccountHolder = isCash ? null : ToNullable(AccountHolder);
            row.OpeningBalance = OpeningBalance;
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MFinancialAccount row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
