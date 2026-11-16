using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Approval;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Information
{
    public partial class ActivityLog
    {
        private static readonly string[] SensitiveKeys = { "password", "token", "secret", "hash", "key", "signature" };
        private const string MASK = "***";

        #region SYNC
        public static List<ActivityLog> GetDataList(int? userId = null, string? module = null, string? action = null, string? entityName = null,
            int? entityId = null, DateTime? from = null, DateTime? to = null, int startdata = -1, int jmldata = 50)
        {
            return Page(FilterQuery(userId, module, action, entityName, entityId, from, to), startdata, jmldata).ToList()
                .MappingData<ActivityLog, EF.TActivityLog>();
        }
        // Tidak butuh login (mis. LOGIN_FAILED); user diambil dari NewRow.UserId.
        public static Response<ActivityLog> Log(NewRow newrow)
        {
            return ServiceHandler.Handle(() =>
            {
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow);
                context.TActivityLogs.Add(row);
                context.SaveChanges();
                return new ActivityLog(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityLog>(msg));
        }
        // Untuk job: hapus log sebelum tanggal tertentu. Mengembalikan jumlah yang dihapus.
        public static int Purge(DateTime before)
        {
            using var context = DBClass.GetContext();
            return context.TActivityLogs.Where(x => x.InsDate < before).ExecuteDelete();
        }
        #endregion

        #region ASYNC
        public static async Task<List<ActivityLog>> GetDataListAsync(int? userId = null, string? module = null, string? action = null, string? entityName = null,
            int? entityId = null, DateTime? from = null, DateTime? to = null, int startdata = -1, int jmldata = 50)
        {
            return await (await Page(FilterQuery(userId, module, action, entityName, entityId, from, to), startdata, jmldata).ToListAsync())
                .MappingDataAsync<ActivityLog, EF.TActivityLog>();
        }
        public static async Task<Response<ActivityLog>> LogAsync(NewRow newrow)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow);
                context.TActivityLogs.Add(row);
                await context.SaveChangesAsync();
                return new ActivityLog(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityLog>(msg));
        }
        public static async Task<int> PurgeAsync(DateTime before)
        {
            using var context = DBClass.GetContext();
            return await context.TActivityLogs.Where(x => x.InsDate < before).ExecuteDeleteAsync();
        }
        #endregion

        #region INTERNAL
        // Tanpa SaveChanges; ikut tersimpan bersama perubahan yang dicatat.
        internal static void Add(EF.ModelContext context, int? userId, string module, string action, string? entityName = null, int? entityId = null,
            object? oldValue = null, object? newValue = null)
        {
            context.TActivityLogs.Add(CreateRow(context, new NewRow
            {
                UserId = userId,
                Module = module,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValue = oldValue,
                NewValue = newValue
            }));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TActivityLog> Page(IQueryable<EF.TActivityLog> query, int startdata, int jmldata)
        {
            return startdata > -1 ? query.Skip(startdata).Take(jmldata) : query;
        }
        private static IQueryable<EF.TActivityLog> FilterQuery(int? userId, string? module, string? action, string? entityName, int? entityId,
            DateTime? from, DateTime? to)
        {
            var moduleCode = string.IsNullOrWhiteSpace(module) ? null : DocumentNumberFormat.ToTypeCode(module);
            var actionCode = string.IsNullOrWhiteSpace(action) ? null : DocumentNumberFormat.ToTypeCode(action);
            var entity = TransaksiHelper.ToNullable(entityName)?.ToLowerInvariant();
            return _Querys.Where(x => (userId == null || x.UserId == userId)
                    && (moduleCode == null || x.Module == moduleCode)
                    && (actionCode == null || x.Action == actionCode)
                    && (entity == null || x.EntityName == entity)
                    && (entityId == null || x.EntityId == entityId)
                    && (from == null || x.InsDate >= from)
                    && (to == null || x.InsDate <= to))
                .OrderByDescending(x => x.InsDate).ThenByDescending(x => x.Id);
        }
        private static EF.TActivityLog CreateRow(EF.ModelContext context, NewRow newrow)
        {
            var module = DocumentNumberFormat.ToTypeCode(newrow.Module);
            var action = DocumentNumberFormat.ToTypeCode(newrow.Action);
            if (module.Length == 0 || action.Length == 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Modul dan Aksi"));
            }
            var userId = newrow.UserId > 0 ? newrow.UserId : null;
            var row = new EF.TActivityLog
            {
                Id = SequenceUtility.GetNextId<EF.TActivityLog>(context),
                UserId = userId,
                Module = TransaksiHelper.Truncate(module, 50)!,
                Action = TransaksiHelper.Truncate(action, 50)!,
                EntityName = TransaksiHelper.Truncate(newrow.EntityName, 100)?.ToLowerInvariant(),
                EntityId = newrow.EntityId,
                OldValue = ToJson(newrow.OldValue),
                NewValue = ToJson(newrow.NewValue),
                IpAddress = TransaksiHelper.Truncate(newrow.IpAddress, 45),
                UserAgent = TransaksiHelper.Truncate(newrow.UserAgent, 255),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            return row;
        }
        // String JSON dipakai apa adanya (setelah disamarkan); string biasa / objek diserialisasi.
        private static string? ToJson(object? value)
        {
            if (value == null)
            {
                return null;
            }
            JsonNode? node;
            if (value is string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }
                try
                {
                    node = JsonNode.Parse(text);
                }
                catch (JsonException)
                {
                    node = JsonValue.Create(text);
                }
            }
            else
            {
                node = JsonSerializer.SerializeToNode(value);
            }
            Mask(node);
            return node?.ToJsonString();
        }
        private static void Mask(JsonNode? node)
        {
            switch (node)
            {
                case JsonObject obj:
                    foreach (var key in obj.Select(x => x.Key).ToList())
                    {
                        if (SensitiveKeys.Any(s => key.Contains(s, StringComparison.OrdinalIgnoreCase)))
                        {
                            obj[key] = MASK;
                        }
                        else
                        {
                            Mask(obj[key]);
                        }
                    }
                    break;
                case JsonArray array:
                    foreach (var item in array)
                    {
                        Mask(item);
                    }
                    break;
            }
        }
        #endregion
    }
}
