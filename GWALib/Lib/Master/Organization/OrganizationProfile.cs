using GWALib.General;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Organization
{
    /// <summary>
    /// Profil organisasi (m_organization), hanya satu baris. Tidak dinamai "Organization" karena sama dengan nama namespace-nya.
    /// Data pembina ikut di sini (hanya dicatat, bukan akun). Logo dan file SK pembina tidak ikut dimuat; ambil lewat GetLogo / GetAdvisorSkFile.
    /// </summary>
    public partial class OrganizationProfile : BaseLib
    {
        public class NewRow
        {
            // Kode singkat untuk placeholder {org} penomoran dokumen, mis. GWA.
            public string Code { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public string? Vision { get; set; }
            public string? Mission { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Instagram { get; set; }
            public DateOnly? FoundedDate { get; set; }
            public string? AdvisorName { get; set; }
            public string? AdvisorPhone { get; set; }
            public string? AdvisorEmail { get; set; }
            public string? AdvisorSkNumber { get; set; }
        }

        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Vision { get; set; }
        public string? Mission { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Instagram { get; set; }
        public DateOnly? FoundedDate { get; set; }
        public bool HasLogo { get; private set; }
        public string? AdvisorName { get; set; }
        public string? AdvisorPhone { get; set; }
        public string? AdvisorEmail { get; set; }
        public string? AdvisorSkNumber { get; set; }
        // Terisi = file SK pembina ada.
        public string? AdvisorSkFileName { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<OrganizationPhoto.Info>? PhotoList;

        // Kolom blob tidak ikut di-select; hanya penanda ada/tidaknya logo.
        private static IQueryable<Snapshot> _Querys
        {
            get
            {
                return DBClass.GetContext().MOrganizations
                    .OrderBy(x => x.Id)
                    .Select(x => new Snapshot(new EF.MOrganization
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Name = x.Name,
                        Description = x.Description,
                        Vision = x.Vision,
                        Mission = x.Mission,
                        Email = x.Email,
                        Phone = x.Phone,
                        Instagram = x.Instagram,
                        FoundedDate = x.FoundedDate,
                        AdvisorName = x.AdvisorName,
                        AdvisorPhone = x.AdvisorPhone,
                        AdvisorEmail = x.AdvisorEmail,
                        AdvisorSkNumber = x.AdvisorSkNumber,
                        AdvisorSkFileName = x.AdvisorSkFileName,
                        InsDate = x.InsDate,
                        InsBy = x.InsBy,
                        UpdDate = x.UpdDate,
                        UpdBy = x.UpdBy
                    }, x.Logo != null));
            }
        }
        private record Snapshot(EF.MOrganization Row, bool HasLogo);

        internal OrganizationProfile(EF.MOrganization row, bool hasLogo)
        {
            SetData(row, hasLogo);
        }
        private void SetData(EF.MOrganization row, bool hasLogo)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<OrganizationProfile>();
            Code = row.Code;
            Name = row.Name;
            Description = row.Description;
            Vision = row.Vision;
            Mission = row.Mission;
            Email = row.Email;
            Phone = row.Phone;
            Instagram = row.Instagram;
            FoundedDate = row.FoundedDate;
            HasLogo = hasLogo;
            AdvisorName = row.AdvisorName;
            AdvisorPhone = row.AdvisorPhone;
            AdvisorEmail = row.AdvisorEmail;
            AdvisorSkNumber = row.AdvisorSkNumber;
            AdvisorSkFileName = row.AdvisorSkFileName;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
