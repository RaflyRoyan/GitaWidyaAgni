using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Inventaris
{
    /// <summary>
    /// Kategori aset organisasi (m_asset_category), mis. Elektronik, Alat Olahraga. Nama unik.
    /// </summary>
    public partial class AssetCategory : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private List<Asset>? AssetList;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MAssetCategory> _Query
        {
            get
            {
                return _context.MAssetCategories
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MAssetCategory> _Querys
        {
            get
            {
                return DBClass.GetContext().MAssetCategories
                    .AsQueryable();
            }
        }
        public AssetCategory()
        {
            _context = DBClass.GetContext();
        }
        public AssetCategory(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<AssetCategory>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal AssetCategory(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal AssetCategory(EF.MAssetCategory row)
        {
            SetData(row);
        }
        private void SetData(EF.MAssetCategory row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<AssetCategory>();
            Name = row.Name;
            Description = row.Description;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
