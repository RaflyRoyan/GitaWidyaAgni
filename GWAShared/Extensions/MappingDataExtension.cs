using GWAShared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GWAShared.Extensions
{
    public static class MappingDataExtension
    {
        #region SYNC
        public static List<C> MappingData<C, O>(this List<O>? rows)
        {
            if (rows == null || rows.Count() == 0) return new List<C>();

            List<C> rets = new();

            ConstructorInfo? ci = typeof(C).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                                                            null, new Type[] { rows[0].GetType() }, null);

            if (ci == null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.CLASS_CONSTRUCTOR_NOT_FOUND,
                                                                typeof(C).Name,
                                                                typeof(O).Name,
                                                                typeof(C).Name
                                                              )
                                               );
            }
            foreach (var item in rows)
            {
                var instance = Activator.CreateInstance(typeof(C),
                                                        BindingFlags.Instance | BindingFlags.NonPublic,
                                                        null,
                                                        new object[] { item }, null
                                                        );

                rets.Add((C)instance!);
            }
            return rets;
        }
        public static C? MappingData<C, O>(this O? row)
        {
            if (row == null)
                return default;

            ConstructorInfo? ci = typeof(C).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(O) },
                null);

            if (ci == null)
            {
                throw new ArgumentException(string.Format(
                    ExceptionMessage.CLASS_CONSTRUCTOR_NOT_FOUND,
                    typeof(C).Name,
                    typeof(O).Name,
                    typeof(C).Name));
            }

            var instance = Activator.CreateInstance(
                typeof(C),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { row },
                null);

            return (C)instance!;
        }
        #endregion

        #region ASYNC
        public static async Task<List<C>> MappingDataAsync<C, O>(this List<O>? rows)
        {
            return await Task.Run(() =>
            {
                if (rows == null || rows.Count == 0)
                    return new List<C>();

                List<C> rets = new();

                ConstructorInfo? ci = typeof(C).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(O) },
                    null);

                if (ci == null)
                {
                    throw new ArgumentException(
                        string.Format(
                            ExceptionMessage.CLASS_CONSTRUCTOR_NOT_FOUND,
                            typeof(C).Name,
                            typeof(O).Name,
                            typeof(C).Name));
                }

                foreach (var item in rows)
                {
                    var instance = Activator.CreateInstance(
                        typeof(C),
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new object[] { item },
                        null);

                    rets.Add((C)instance!);
                }

                return rets;
            });
        }
        public static async Task<C?> MappingDataAsync<C, O>(this O? row)
        {
            return await Task.Run(() =>
            {
                if (row == null)
                    return default;

                ConstructorInfo? ci = typeof(C).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(O) },
                    null);

                if (ci == null)
                {
                    throw new ArgumentException(string.Format(
                        ExceptionMessage.CLASS_CONSTRUCTOR_NOT_FOUND,
                        typeof(C).Name,
                        typeof(O).Name,
                        typeof(C).Name));
                }

                var instance = Activator.CreateInstance(
                    typeof(C),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { row },
                    null);

                return (C)instance!;
            });
        }
        #endregion
    }
}
