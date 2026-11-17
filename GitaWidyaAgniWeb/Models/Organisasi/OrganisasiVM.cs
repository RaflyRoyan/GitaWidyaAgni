using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Organization;

namespace GitaWidyaAgniWeb.Models.Organisasi
{
    /// <summary>
    /// Organisasi › Profil Organisasi: satu baris m_organization (profil, pembina, logo, SK pembina) dan foto organisasi.
    /// Form tampil langsung di halaman; simpan pertama membuat profil, berikutnya mengubahnya.
    /// </summary>
    public class OrganisasiVM
    {
        public class Index
        {
            public NewRow Row { get; set; } = new();
            public bool SudahAda { get; set; }
            public bool HasLogo { get; set; }
            public string? SkFileName { get; set; }

            public Index()
            {
                var obj = OrganizationProfile.GetData();
                if (obj == null)
                {
                    return;
                }
                SudahAda = true;
                HasLogo = obj.HasLogo;
                SkFileName = obj.AdvisorSkFileName;
                Row = new NewRow
                {
                    Code = obj.Code,
                    Name = obj.Name,
                    Description = obj.Description,
                    Vision = obj.Vision,
                    Mission = obj.Mission,
                    Email = obj.Email,
                    Phone = obj.Phone,
                    Instagram = obj.Instagram,
                    FoundedDate = Opsi.KeDateTime(obj.FoundedDate),
                    AdvisorName = obj.AdvisorName,
                    AdvisorPhone = obj.AdvisorPhone,
                    AdvisorEmail = obj.AdvisorEmail,
                    AdvisorSkNumber = obj.AdvisorSkNumber
                };
            }
        }

        public class NewRow
        {
            [Required(ErrorMessage = "Kode harus diisi")]
            [StringLength(20, ErrorMessage = "Kode maksimal 20 karakter")]
            [Display(Name = "Kode (untuk nomor dokumen)")]
            public string? Code { get; set; }

            [Required(ErrorMessage = "Nama organisasi harus diisi")]
            [StringLength(150, ErrorMessage = "Nama maksimal 150 karakter")]
            [Display(Name = "Nama Organisasi")]
            public string? Name { get; set; }

            [Display(Name = "Deskripsi")]
            public string? Description { get; set; }

            [Display(Name = "Visi")]
            public string? Vision { get; set; }

            [Display(Name = "Misi (satu per baris)")]
            public string? Mission { get; set; }

            [EmailAddress(ErrorMessage = "Format email tidak valid")]
            [StringLength(100, ErrorMessage = "Email maksimal 100 karakter")]
            [Display(Name = "Email")]
            public string? Email { get; set; }

            [StringLength(30, ErrorMessage = "Telepon maksimal 30 karakter")]
            [Display(Name = "Telepon")]
            public string? Phone { get; set; }

            [StringLength(100, ErrorMessage = "Instagram maksimal 100 karakter")]
            [Display(Name = "Instagram")]
            public string? Instagram { get; set; }

            [Display(Name = "Tanggal Berdiri")]
            public DateTime? FoundedDate { get; set; }

            [StringLength(150, ErrorMessage = "Nama pembina maksimal 150 karakter")]
            [Display(Name = "Nama Pembina")]
            public string? AdvisorName { get; set; }

            [StringLength(30, ErrorMessage = "Telepon pembina maksimal 30 karakter")]
            [Display(Name = "Telepon Pembina")]
            public string? AdvisorPhone { get; set; }

            [EmailAddress(ErrorMessage = "Format email pembina tidak valid")]
            [StringLength(100, ErrorMessage = "Email pembina maksimal 100 karakter")]
            [Display(Name = "Email Pembina")]
            public string? AdvisorEmail { get; set; }

            [StringLength(100, ErrorMessage = "Nomor SK maksimal 100 karakter")]
            [Display(Name = "Nomor SK Pembina")]
            public string? AdvisorSkNumber { get; set; }
        }

        public class Foto
        {
            public List<FotoItem> Items { get; set; } = OrganizationPhoto.GetInfoList()
                .OrderBy(x => x.SeqPhoto)
                .Select(x => new FotoItem { SeqPhoto = x.SeqPhoto, Ukuran = x.FileSize, Versi = x.Stamp.Ticks })
                .ToList();
        }

        public class FotoItem
        {
            public int SeqPhoto { get; set; }
            public int Ukuran { get; set; }
            public long Versi { get; set; }
        }

        public class FotoInput
        {
            public int SeqPhoto { get; set; }
        }

        public class Method
        {
            public static void Simpan(NewRow input, byte[]? logo, bool hapusLogo, byte[]? sk, string? skName, bool hapusSk, string user)
            {
                var obj = OrganizationProfile.GetData();
                if (obj == null)
                {
                    obj = OrganizationProfile.Save(new OrganizationProfile.NewRow
                    {
                        Code = input.Code ?? "",
                        Name = input.Name ?? "",
                        Description = input.Description,
                        Vision = input.Vision,
                        Mission = input.Mission,
                        Email = input.Email,
                        Phone = input.Phone,
                        Instagram = input.Instagram,
                        FoundedDate = Opsi.Tanggal(input.FoundedDate),
                        AdvisorName = input.AdvisorName,
                        AdvisorPhone = input.AdvisorPhone,
                        AdvisorEmail = input.AdvisorEmail,
                        AdvisorSkNumber = input.AdvisorSkNumber
                    }, user).Ok();
                }
                else
                {
                    obj.Code = input.Code ?? "";
                    obj.Name = input.Name ?? "";
                    obj.Description = input.Description;
                    obj.Vision = input.Vision;
                    obj.Mission = input.Mission;
                    obj.Email = input.Email;
                    obj.Phone = input.Phone;
                    obj.Instagram = input.Instagram;
                    obj.FoundedDate = Opsi.Tanggal(input.FoundedDate);
                    obj.AdvisorName = input.AdvisorName;
                    obj.AdvisorPhone = input.AdvisorPhone;
                    obj.AdvisorEmail = input.AdvisorEmail;
                    obj.AdvisorSkNumber = input.AdvisorSkNumber;
                    obj = obj.Update(user).Ok();
                }
                if (logo != null || hapusLogo)
                {
                    obj.UpdateLogo(logo, user).Ok();
                }
                if (sk != null || hapusSk)
                {
                    obj.UpdateAdvisorSkFile(sk, sk == null ? null : skName, user).Ok();
                }
            }

            public static void TambahFoto(byte[]? data, string user)
            {
                if (data == null)
                {
                    throw new ArgumentException("Pilih foto terlebih dahulu.");
                }
                OrganizationPhoto.Save(new OrganizationPhoto.NewRow { Data = data }, user).Ok();
            }

            public static void HapusFoto(int seqPhoto, string user)
            {
                var obj = OrganizationPhoto.GetDataList().FirstOrDefault(x => x.SeqPhoto == seqPhoto)
                    ?? throw new ArgumentException("Foto tidak ditemukan.");
                obj.Delete(user).Ok();
            }
        }
    }
}
