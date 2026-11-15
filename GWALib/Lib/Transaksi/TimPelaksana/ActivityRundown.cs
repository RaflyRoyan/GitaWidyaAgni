using GWALib.General;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.TimPelaksana
{
    /// <summary>
    /// Rundown / susunan acara kegiatan (t_activity_rundown), bisa beberapa hari (mis. lomba luar kota).
    /// Urut per tanggal, jam mulai, lalu SeqNo. Sie penanggung jawab harus sie kegiatan yang sama.
    /// </summary>
    public partial class ActivityRundown : BaseLib
    {
        public class NewRow
        {
            public int ActivityId { get; set; }
            public DateOnly RundownDate { get; set; }
            public TimeOnly StartTime { get; set; }
            public TimeOnly? EndTime { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public string? LocationName { get; set; }
            public int? SectionId { get; set; }
            public int? PicMemberId { get; set; }
            // 0 = otomatis ditaruh paling akhir pada tanggal itu.
            public int SeqNo { get; set; }
        }

        public int Id { get; set; }
        public int ActivityId { get; private set; }
        public DateOnly RundownDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? LocationName { get; set; }
        public int? SectionId { get; set; }
        public string? SectionName { get; private set; }
        public int? PicMemberId { get; set; }
        public string? PicMemberName { get; private set; }
        public int SeqNo { get; set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TActivityRundown> _Querys
        {
            get
            {
                return DBClass.GetContext().TActivityRundowns.Include(x => x.Section).Include(x => x.PicMember).AsQueryable();
            }
        }
        public ActivityRundown()
        {
        }
        public ActivityRundown(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ActivityRundown>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ActivityRundown(EF.TActivityRundown row)
        {
            SetData(row);
        }
        private void SetData(EF.TActivityRundown row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ActivityRundown>();
            ActivityId = row.ActivityId;
            RundownDate = row.RundownDate;
            StartTime = row.StartTime;
            EndTime = row.EndTime;
            Title = row.Title;
            Description = row.Description;
            LocationName = row.LocationName;
            SectionId = row.SectionId;
            SectionName = row.Section?.Name;
            PicMemberId = row.PicMemberId;
            PicMemberName = row.PicMember?.Fullname;
            SeqNo = row.SeqNo;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
