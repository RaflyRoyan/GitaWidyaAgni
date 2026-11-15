using GWALib.General;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Sponsorship
{
    /// <summary>
    /// Benefit bawaan paket sponsorship (m_sponsorship_package_benefit), mis. Logo di banner utama. SeqNo urutan dalam paket.
    /// Hanya katalog: benefit per sponsorship (t_sponsorship_benefit) disalin terpisah, jadi benefit paket boleh dihapus.
    /// </summary>
    public partial class PackageBenefit : BaseLib
    {
        public class NewRow
        {
            public int PackageId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            // 0 = otomatis ditaruh paling akhir dalam paket.
            public int SeqNo { get; set; }
        }

        public int Id { get; set; }
        public int PackageId { get; private set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int SeqNo { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private SponsorshipPackage? PackageData;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MSponsorshipPackageBenefit> _Query
        {
            get
            {
                return _context.MSponsorshipPackageBenefits.AsQueryable();
            }
        }
        private static IQueryable<EF.MSponsorshipPackageBenefit> _Querys
        {
            get
            {
                return DBClass.GetContext().MSponsorshipPackageBenefits.AsQueryable();
            }
        }
        public PackageBenefit()
        {
            _context = DBClass.GetContext();
        }
        public PackageBenefit(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<PackageBenefit>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal PackageBenefit(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal PackageBenefit(EF.MSponsorshipPackageBenefit row)
        {
            SetData(row);
        }
        private void SetData(EF.MSponsorshipPackageBenefit row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<PackageBenefit>();
            PackageId = row.PackageId;
            Title = row.Title;
            Description = row.Description;
            SeqNo = row.SeqNo;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
