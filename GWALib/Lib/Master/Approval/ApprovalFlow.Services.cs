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

namespace GWALib.Lib.Master.Approval
{
    public partial class ApprovalFlow
    {
        #region SYNC
        public List<ApprovalFlowStep> GetSteps(bool refresh)
        {
            if (refresh || StepList == null)
            {
                StepList = ApprovalFlowStep.GetDataList(flowId: Id);
            }
            return StepList;
        }
        public static ApprovalFlow? GetEffective(string documentType)
        {
            var row = EffectiveQuery(DocumentNumberFormat.ToTypeCode(documentType)).FirstOrDefault();
            return row != null ? new ApprovalFlow(row) : null;
        }
        public static List<ApprovalFlow> GetDataList(FilterAktif aktif, string? documentType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, documentType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ApprovalFlow, EF.MApprovalFlow>();
        }
        public static Response<ApprovalFlow> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new ApprovalFlow(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ApprovalFlow>(msg));
        }
        public Response<ApprovalFlow> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ApprovalFlow(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ApprovalFlow>(msg));
        }
        public Response<ApprovalFlow> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new ApprovalFlow(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ApprovalFlow>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<ApprovalFlowStep>> GetStepsAsync(bool refresh)
        {
            if (refresh || StepList == null)
            {
                StepList = await ApprovalFlowStep.GetDataListAsync(flowId: Id);
            }
            return StepList;
        }
        public static async Task<ApprovalFlow?> GetEffectiveAsync(string documentType)
        {
            var row = await EffectiveQuery(DocumentNumberFormat.ToTypeCode(documentType)).FirstOrDefaultAsync();
            return row != null ? new ApprovalFlow(row) : null;
        }
        public static async Task<List<ApprovalFlow>> GetDataListAsync(FilterAktif aktif, string? documentType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, documentType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ApprovalFlow, EF.MApprovalFlow>();
        }
        public static async Task<Response<ApprovalFlow>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new ApprovalFlow(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ApprovalFlow>(msg));
        }
        public async Task<Response<ApprovalFlow>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ApprovalFlow(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ApprovalFlow>(msg));
        }
        public async Task<Response<ApprovalFlow>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new ApprovalFlow(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ApprovalFlow>(msg));
        }
        #endregion

        #region HELPER
        // Alur tidak harus aktif: langkah boleh disiapkan pada alur yang belum diaktifkan.
        // Local: alur yang baru di-Add dalam command yang sama (Save dengan Steps) belum ada di DB.
        internal static EF.MApprovalFlow GetFlow(EF.ModelContext context, int flowId)
        {
            return context.MApprovalFlows.Local.SingleOrDefault(x => x.Id == flowId)
                ?? context.MApprovalFlows.SingleOrDefault(x => x.Id == flowId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Alur Persetujuan"));
        }
        private static IQueryable<EF.MApprovalFlow> EffectiveQuery(string documentType)
        {
            return _Querys.Where(x => x.DocumentType == documentType && x.IsActive == (int)ActiveStatus.Aktif);
        }
        private static IQueryable<EF.MApprovalFlow> FilterQuery(FilterAktif aktif, string? documentType)
        {
            var type = string.IsNullOrWhiteSpace(documentType) ? null : DocumentNumberFormat.ToTypeCode(documentType);
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (type == null || x.DocumentType == type))
                .OrderBy(x => x.DocumentType).ThenBy(x => x.Name);
        }
        private static EF.MApprovalFlow GetRow(EF.ModelContext context, int id)
        {
            return context.MApprovalFlows.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Alur Persetujuan"));
        }
        private static async Task<EF.MApprovalFlow> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MApprovalFlows.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Alur Persetujuan"));
        }
        private static void ValidateData(string name, ActiveStatus isActive)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Alur"));
            }
            if (!Enum.IsDefined(isActive))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status"));
            }
        }
        // Dicek di sini agar pesannya jelas; uq_approval_flow_active di DB tetap menjadi pengaman terakhir.
        private static void ValidateSingleActive(EF.ModelContext context, int id, string documentType)
        {
            if (context.MApprovalFlows.Any(x => x.DocumentType == documentType
                && x.IsActive == (int)ActiveStatus.Aktif && x.Id != id))
            {
                throw new ArgumentException(ExceptionMessage.APPROVAL_FLOW_ACTIVE_EXISTS);
            }
        }
        private static EF.MApprovalFlow CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var documentType = DocumentNumberFormat.ToTypeCode(newrow.DocumentType);
            var name = newrow.Name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(documentType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Jenis Dokumen"));
            }
            ValidateData(name, newrow.IsActive);
            if (newrow.IsActive == ActiveStatus.Aktif)
            {
                ValidateSingleActive(context, 0, documentType);
                if (newrow.Steps == null || newrow.Steps.Count == 0)
                {
                    throw new ArgumentException(ExceptionMessage.APPROVAL_FLOW_STEP_REQUIRED);
                }
            }

            var row = new EF.MApprovalFlow
            {
                Id = SequenceUtility.GetNextId<EF.MApprovalFlow>(context),
                DocumentType = documentType,
                Name = name,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                IsActive = (int)newrow.IsActive,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.MApprovalFlows.Add(row);

            foreach (var step in newrow.Steps ?? new())
            {
                step.FlowId = row.Id;
                context.MApprovalFlowSteps.Add(ApprovalFlowStep.CreateRow(context, step, userId));
            }
            return row;
        }
        // Jenis dokumen tetap (langkah dan persetujuan yang berjalan mengacu ke alur ini).
        private void SetRow(EF.ModelContext context, EF.MApprovalFlow row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(name, IsActive);
            if (IsActive == ActiveStatus.Aktif && row.IsActive != (int)ActiveStatus.Aktif)
            {
                ValidateSingleActive(context, row.Id, row.DocumentType);
                if (!context.MApprovalFlowSteps.Any(x => x.FlowId == row.Id))
                {
                    throw new ArgumentException(ExceptionMessage.APPROVAL_FLOW_STEP_REQUIRED);
                }
            }

            row.Name = name;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MApprovalFlow row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
