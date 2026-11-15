using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Anggota;
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
    public partial class ApprovalFlowStep
    {
        #region SYNC
        public static List<ApprovalFlowStep> GetDataList(int? flowId = null, ApproverType? approverType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(flowId, approverType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ApprovalFlowStep, EF.MApprovalFlowStep>();
        }
        public static Response<ApprovalFlowStep> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MApprovalFlowSteps.Add(row);
                context.SaveChanges();
                return new ApprovalFlowStep(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ApprovalFlowStep>(msg));
        }
        public Response<ApprovalFlowStep> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ApprovalFlowStep(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ApprovalFlowStep>(msg));
        }
        public Response<ApprovalFlowStep> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateDelete(context, row);
                context.MApprovalFlowSteps.Remove(row);
                context.SaveChanges();
                return new ApprovalFlowStep(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ApprovalFlowStep>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ApprovalFlowStep>> GetDataListAsync(int? flowId = null, ApproverType? approverType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(flowId, approverType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ApprovalFlowStep, EF.MApprovalFlowStep>();
        }
        public static async Task<Response<ApprovalFlowStep>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MApprovalFlowSteps.Add(row);
                await context.SaveChangesAsync();
                return new ApprovalFlowStep(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ApprovalFlowStep>(msg));
        }
        public async Task<Response<ApprovalFlowStep>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ApprovalFlowStep(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ApprovalFlowStep>(msg));
        }
        public async Task<Response<ApprovalFlowStep>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateDelete(context, row);
                context.MApprovalFlowSteps.Remove(row);
                await context.SaveChangesAsync();
                return new ApprovalFlowStep(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ApprovalFlowStep>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.MApprovalFlowStep> FilterQuery(int? flowId, ApproverType? approverType)
        {
            return _Querys.Where(x => (flowId == null || x.FlowId == flowId)
                    && (approverType == null || x.ApproverType == (int)approverType))
                .OrderBy(x => x.FlowId).ThenBy(x => x.StepLevel);
        }
        private static EF.MApprovalFlowStep GetRow(EF.ModelContext context, int id)
        {
            return context.MApprovalFlowSteps.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Langkah Persetujuan"));
        }
        private static async Task<EF.MApprovalFlowStep> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MApprovalFlowSteps.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Langkah Persetujuan"));
        }
        // Local: langkah lain yang di-Add bersama alur (ApprovalFlow.Save dengan Steps) belum ada di DB.
        private static void ValidateData(EF.ModelContext context, int id, int flowId, int stepLevel, string stepName, RevisionPermission allowRevision)
        {
            if (string.IsNullOrWhiteSpace(stepName))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Langkah"));
            }
            if (stepLevel <= 0)
            {
                throw new ArgumentException(ExceptionMessage.APPROVAL_STEP_LEVEL_INVALID);
            }
            if (!Enum.IsDefined(allowRevision))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Izin Revisi"));
            }
            var duplicateMsg = string.Format(ExceptionMessage.DATA_DUPLICATE, "Urutan Langkah");
            if (context.MApprovalFlowSteps.Local.Any(x => x.FlowId == flowId && x.StepLevel == stepLevel && x.Id != id))
            {
                throw new ArgumentException(duplicateMsg);
            }
            ValidateUtility.CheckDuplicateData<EF.MApprovalFlowStep>(context,
                x => x.FlowId == flowId && x.StepLevel == stepLevel && x.Id != id, duplicateMsg);
        }
        // Mengosongkan kolom approver yang tidak dipakai ApproverType, lalu memvalidasi yang dipakai (harus ada dan aktif).
        private static (int? RoleId, int? PositionId, int? UserId) ResolveApprover(EF.ModelContext context,
            ApproverType approverType, int? roleId, int? positionId, int? userId)
        {
            if (!Enum.IsDefined(approverType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Approver"));
            }
            roleId = approverType == ApproverType.Role && roleId > 0 ? roleId : null;
            positionId = approverType == ApproverType.Jabatan && positionId > 0 ? positionId : null;
            userId = approverType == ApproverType.UserTertentu && userId > 0 ? userId : null;

            switch (approverType)
            {
                case ApproverType.Role:
                    if (roleId == null)
                    {
                        throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Role"));
                    }
                    Role.GetActiveRole(context, roleId.Value);
                    break;
                case ApproverType.Jabatan:
                    if (positionId == null)
                    {
                        throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Jabatan"));
                    }
                    Position.ValidateActivePosition(context, positionId.Value);
                    break;
                case ApproverType.UserTertentu:
                    if (userId == null)
                    {
                        throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "User"));
                    }
                    if (UserAccount.GetUser(context, userId.Value).IsActive != (int)ActiveStatus.Aktif)
                    {
                        throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "User"));
                    }
                    break;
            }
            return (roleId, positionId, userId);
        }
        // Langkah yang sudah dipakai persetujuan dokumen tidak boleh dihapus; langkah terakhir alur aktif juga tidak.
        private static void ValidateDelete(EF.ModelContext context, EF.MApprovalFlowStep row)
        {
            if (context.TFundingApprovals.Any(x => x.FlowStepId == row.Id)
                || context.TLetterApprovals.Any(x => x.FlowStepId == row.Id)
                || context.TProcurementApprovals.Any(x => x.FlowStepId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.APPROVAL_STEP_IN_USE);
            }
            var flow = ApprovalFlow.GetFlow(context, row.FlowId);
            if (flow.IsActive == (int)ActiveStatus.Aktif && !context.MApprovalFlowSteps.Any(x => x.FlowId == row.FlowId && x.Id != row.Id))
            {
                throw new ArgumentException(ExceptionMessage.APPROVAL_FLOW_STEP_REQUIRED);
            }
        }
        // Tanpa SaveChanges; dipakai juga oleh ApprovalFlow.Save untuk membuat langkah bersama alurnya.
        internal static EF.MApprovalFlowStep CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var stepName = newrow.StepName?.Trim() ?? string.Empty;
            ApprovalFlow.GetFlow(context, newrow.FlowId);
            var stepLevel = newrow.StepLevel > 0 ? newrow.StepLevel
                : ValueUtility.GetNextId<EF.MApprovalFlowStep>(context, x => x.StepLevel, x => x.FlowId == newrow.FlowId);
            ValidateData(context, 0, newrow.FlowId, stepLevel, stepName, newrow.AllowRevision);
            var approver = ResolveApprover(context, newrow.ApproverType, newrow.RoleId, newrow.PositionId, newrow.UserId);

            var row = new EF.MApprovalFlowStep
            {
                Id = SequenceUtility.GetNextId<EF.MApprovalFlowStep>(context),
                FlowId = newrow.FlowId,
                StepLevel = stepLevel,
                StepName = stepName,
                ApproverType = (int)newrow.ApproverType,
                RoleId = approver.RoleId,
                PositionId = approver.PositionId,
                UserId = approver.UserId,
                AllowRevision = (int)newrow.AllowRevision,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Alur tidak bisa dipindah.
        private void SetRow(EF.ModelContext context, EF.MApprovalFlowStep row, int userId)
        {
            var stepName = StepName?.Trim() ?? string.Empty;
            var stepLevel = StepLevel > 0 ? StepLevel
                : ValueUtility.GetNextId<EF.MApprovalFlowStep>(context, x => x.StepLevel, x => x.FlowId == row.FlowId);
            ValidateData(context, row.Id, row.FlowId, stepLevel, stepName, AllowRevision);
            var approver = ResolveApprover(context, ApproverType, Role?.Id, Position?.Id, User?.Id);

            row.StepLevel = stepLevel;
            row.StepName = stepName;
            row.ApproverType = (int)ApproverType;
            row.RoleId = approver.RoleId;
            row.PositionId = approver.PositionId;
            row.UserId = approver.UserId;
            row.AllowRevision = (int)AllowRevision;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
