using GWAShared.Enums;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.General.Utilities
{
    public static class ValidateUtility
    {
        public static int ValidateUser(string user)
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                throw new ArgumentException("User Login tidak valid!");
            }
            if (!int.TryParse(user, out var userId))
            {
                throw new ArgumentException("User Login tidak valid!");
            }

            using var context = DBClass.GetContext();
            var status = context.MUsers.Where(x => x.Id == userId).Select(x => (int?)x.IsActive).FirstOrDefault()
                ?? throw new ArgumentException("User Login tidak valid!");
            if (status != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException("User Login tidak aktif!");
            }
            return userId;
        }
        public static void CheckDuplicateData<O>(DbContext ctx, Expression<Func<O, bool>> expression, string errorMsg)
        where O : class
        {
            int intCount = ctx.Set<O>().Where(expression).Count();
            if (intCount > 0)
            {
                throw new ArgumentException(errorMsg);
            }
        }
    }
}
