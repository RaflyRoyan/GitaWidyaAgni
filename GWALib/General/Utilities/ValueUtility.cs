using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.General.Utilities
{
    public static class ValueUtility
    {
        public static void CopyValueFromCols(this DbContext context, object fromObj, object toObj, List<string> colsName)
        {
            var fromProps = fromObj.GetType().GetProperties();
            var toProps = toObj.GetType().GetProperties();
            var ctxProps = context.Entry(toObj).Properties;
            foreach (var item in colsName)
            {
                var fromprop = fromProps.Where(x => x.Name.ToLower() == item.ToLower()).FirstOrDefault();
                if (fromprop is not null)
                {
                    var objVal = fromprop.GetValue(fromObj);
                    var toProp = toProps.Where(x => x.Name.ToLower() == item.ToLower()).FirstOrDefault()!;
                    GWAShared.Extensions.ValueExtension.SetValue(objVal, toObj, toProp);

                    var prop = context.Entry(toObj).Property(toProp.Name);

                    if (objVal!.GetType().IsEnum && prop.CurrentValue is int?)
                    {
                        prop.CurrentValue = (int)objVal;
                    }
                    else
                    {
                        prop.CurrentValue = objVal;
                    }
                    prop.IsModified = true;
                }
            }
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
        public static int GetMaxValueSpecifyColumn<T>(DbContext ctx, Expression<Func<T, bool>>? expression, string propertyNameEntityEf = "Id")
        where T : class
        {
            var query = ctx.Set<T>().AsQueryable();
            if (expression != null)
            {
                query = query.Where(expression);
            }
            var propertyinfo = typeof(T).GetProperty(propertyNameEntityEf);
            if (propertyinfo == null)
            {
                throw new ArgumentException($"Property '{propertyNameEntityEf}' not found in entity '{typeof(T).Name}'.");
            }
            var maxEntity = query.Select(entity => new { Value = propertyinfo.GetValue(entity) })
                     .AsEnumerable() // Switch to client-side evaluation
                     .Max(x => (int?)x.Value);
            return maxEntity ?? 0;
        }
        /// <summary>
        /// Nilai urutan berikutnya untuk kolom urutan per induk (mis. seq_photo, step_level). Primary key id memakai
        /// <see cref="SequenceUtility.GetNextId{T}"/> (m_sequence), bukan method ini. Menghitung max di database (di-translate ke SQL) dan juga entity yang sudah
        /// di-Add ke context tapi belum di-SaveChanges, supaya beberapa insert dalam satu transaksi tidak dapat nilai yang sama.
        /// </summary>
        public static int GetNextId<T>(DbContext ctx, Expression<Func<T, int>> selector, Expression<Func<T, bool>>? filter = null)
        where T : class
        {
            var query = ctx.Set<T>().AsQueryable();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            var nullableSelector = Expression.Lambda<Func<T, int?>>(Expression.Convert(selector.Body, typeof(int?)), selector.Parameters);
            var dbMax = query.Max(nullableSelector) ?? 0;

            var compiledFilter = filter?.Compile();
            var compiledSelector = selector.Compile();
            var localMax = ctx.Set<T>().Local
                .Where(x => compiledFilter == null || compiledFilter(x))
                .Select(compiledSelector)
                .DefaultIfEmpty(0)
                .Max();

            return Math.Max(dbMax, localMax) + 1;
        }
    }
}
