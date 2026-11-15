using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Approval
{
    /// <summary>
    /// Log eksekusi job latar belakang (t_scheduled_job_log), mis. SEND_OUTBOX, REMIND_CHECKLIST, EXPIRE_MOU.
    /// Satu baris per eksekusi: Start → Berjalan, lalu Finish (Sukses) atau Fail (Gagal).
    /// </summary>
    public partial class ScheduledJobLog : BaseLib
    {
        public int Id { get; private set; }
        public string JobName { get; private set; } = null!;
        public DateTime StartedAt { get; private set; }
        public DateTime? FinishedAt { get; private set; }
        public ScheduledJobStatus Status { get; private set; }
        public int ProcessedCount { get; private set; }
        public string? Message { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        public string StatusNama => Status.GetDescription();
        public TimeSpan? Duration => FinishedAt - StartedAt;

        private static IQueryable<EF.TScheduledJobLog> _Querys
        {
            get
            {
                return DBClass.GetContext().TScheduledJobLogs.AsQueryable();
            }
        }
        public ScheduledJobLog()
        {
        }
        public ScheduledJobLog(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ScheduledJobLog>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ScheduledJobLog(int id)
        {
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ScheduledJobLog(EF.TScheduledJobLog row)
        {
            SetData(row);
        }
        private void SetData(EF.TScheduledJobLog row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ScheduledJobLog>();
            JobName = row.JobName;
            StartedAt = row.StartedAt;
            FinishedAt = row.FinishedAt;
            Status = (ScheduledJobStatus)row.Status;
            ProcessedCount = row.ProcessedCount;
            Message = row.Message;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
