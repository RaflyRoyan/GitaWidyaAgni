using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Sponsorship
{
    /// <summary>
    /// Paket sponsorship yang ditawarkan organisasi (m_sponsorship_package), mis. Paket 1 / Gold. Nama unik; SeqNo urutan tampil.
    /// Benefit paket ada di <see cref="PackageBenefit"/> dan bisa dibuat bersama paketnya.
    /// </summary>
    public partial class SponsorshipPackage : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public decimal Price { get; set; }
            // 0 = otomatis ditaruh paling akhir.
            public int SeqNo { get; set; }
            // Opsional: benefit yang langsung dibuat bersama paket (PackageId diisi otomatis).
            public List<PackageBenefit.NewRow>? Benefits { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int SeqNo { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private List<PackageBenefit>? BenefitList;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MSponsorshipPackage> _Query
        {
            get
            {
                return _context.MSponsorshipPackages.AsQueryable();
            }
        }
        private static IQueryable<EF.MSponsorshipPackage> _Querys
        {
            get
            {
                return DBClass.GetContext().MSponsorshipPackages.AsQueryable();
            }
        }
        public SponsorshipPackage()
        {
            _context = DBClass.GetContext();
        }
        public SponsorshipPackage(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<SponsorshipPackage>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal SponsorshipPackage(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal SponsorshipPackage(EF.MSponsorshipPackage row)
        {
            SetData(row);
        }
        private void SetData(EF.MSponsorshipPackage row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<SponsorshipPackage>();
            Name = row.Name;
            Price = row.Price;
            SeqNo = row.SeqNo;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
