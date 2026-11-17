using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Mitra;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Kemitraan
{
    /// <summary>
    /// Kemitraan › Mitra (m_external_party + kontak). Nama unik; status Blacklist menolak mitra di sponsorship, kolaborasi, dan peminjaman.
    /// Satu kontak utama aktif per mitra.
    /// </summary>
    public class MitraVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = ExternalParty.GetDataList().Select(x => new Row
            {
                EncDecrKey = x.EncDecrKey,
                Name = x.Name,
                Jenis = EnumHelper.Text(x.PartyType),
                Industry = x.Industry,
                City = x.City,
                Phone = x.Phone,
                Email = x.Email,
                Instagram = x.Instagram,
                KontakUtama = x.GetContacts(true).FirstOrDefault(c => c.IsPrimary == ContactRank.Utama && c.IsActive == ActiveStatus.Aktif)?.Name,
                HasLogo = x.HasLogo,
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status)
            }).OrderBy(x => x.Name).ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string Jenis { get; set; } = "";
            public string? Industry { get; set; }
            public string? City { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public string? Instagram { get; set; }
            public string? KontakUtama { get; set; }
            public bool HasLogo { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public bool Aktif => Status == (int)ExternalPartyStatus.Aktif;
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<ExternalPartyType>();
            public List<GwaVM.OpsiItem> StatusOpsi { get; set; } = GwaVM.Opsi<ExternalPartyStatus>();
            public bool HasLogo { get; set; }
        }

        public class Edit : Add
        {
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<ExternalParty>(encKey, "Mitra");
                HasLogo = obj.HasLogo;
                Row = new NewRow
                {
                    Name = obj.Name,
                    PartyType = (int)obj.PartyType,
                    Industry = obj.Industry,
                    Address = obj.Address,
                    City = obj.City,
                    Phone = obj.Phone,
                    Email = obj.Email,
                    Website = obj.Website,
                    Instagram = obj.Instagram,
                    Notes = obj.Notes,
                    Status = (int)obj.Status
                };
            }
        }

        public class NewRow
        {
            [Required(ErrorMessage = "Nama mitra harus diisi")]
            [StringLength(150, ErrorMessage = "Nama maksimal 150 karakter")]
            [Display(Name = "Nama Mitra")]
            public string? Name { get; set; }

            [Display(Name = "Jenis")]
            public int PartyType { get; set; } = (int)ExternalPartyType.Perusahaan;

            [StringLength(100, ErrorMessage = "Bidang maksimal 100 karakter")]
            [Display(Name = "Bidang Usaha")]
            public string? Industry { get; set; }

            [StringLength(255, ErrorMessage = "Alamat maksimal 255 karakter")]
            [Display(Name = "Alamat")]
            public string? Address { get; set; }

            [StringLength(100, ErrorMessage = "Kota maksimal 100 karakter")]
            [Display(Name = "Kota")]
            public string? City { get; set; }

            [StringLength(30, ErrorMessage = "Telepon maksimal 30 karakter")]
            [Display(Name = "Telepon")]
            public string? Phone { get; set; }

            [EmailAddress(ErrorMessage = "Format email tidak valid")]
            [StringLength(100, ErrorMessage = "Email maksimal 100 karakter")]
            [Display(Name = "Email")]
            public string? Email { get; set; }

            [StringLength(255, ErrorMessage = "Website maksimal 255 karakter")]
            [Display(Name = "Website")]
            public string? Website { get; set; }

            [StringLength(100, ErrorMessage = "Instagram maksimal 100 karakter")]
            [Display(Name = "Instagram")]
            public string? Instagram { get; set; }

            [Display(Name = "Catatan")]
            public string? Notes { get; set; }

            [Display(Name = "Status")]
            public int Status { get; set; } = (int)ExternalPartyStatus.Aktif;
        }

        // Kontak (modal _Kontak).
        public class Kontak
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public KontakInput Input { get; set; } = new();
            public List<GwaVM.OpsiItem> RankOpsi { get; set; } = GwaVM.Opsi<ContactRank>();
            public List<KontakRow> Rows { get; set; }

            public Kontak(string encKey)
            {
                var obj = LibExtensions.Load<ExternalParty>(encKey, "Mitra");
                Key = obj.EncDecrKey;
                Nama = obj.Name;
                Rows = obj.GetContacts(true).Select(x => new KontakRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Name = x.Name,
                    Position = x.Position,
                    Phone = x.Phone,
                    Email = x.Email,
                    Utama = x.IsPrimary == ContactRank.Utama,
                    Aktif = x.IsActive == ActiveStatus.Aktif,
                    Notes = x.Notes
                }).OrderByDescending(x => x.Aktif).ThenByDescending(x => x.Utama).ThenBy(x => x.Name).ToList();
            }
        }

        public class KontakRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Position { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public bool Utama { get; set; }
            public bool Aktif { get; set; }
            public string? Notes { get; set; }
        }

        public class KontakInput
        {
            public string Key { get; set; } = "";

            [Required(ErrorMessage = "Nama kontak harus diisi")]
            [StringLength(150, ErrorMessage = "Nama maksimal 150 karakter")]
            [Display(Name = "Nama")]
            public string? Name { get; set; }

            [StringLength(100, ErrorMessage = "Jabatan maksimal 100 karakter")]
            [Display(Name = "Jabatan")]
            public string? Position { get; set; }

            [StringLength(30, ErrorMessage = "Telepon maksimal 30 karakter")]
            [Display(Name = "Telepon")]
            public string? Phone { get; set; }

            [StringLength(100, ErrorMessage = "Email maksimal 100 karakter")]
            [Display(Name = "Email")]
            public string? Email { get; set; }

            [Display(Name = "Kontak")]
            public int IsPrimary { get; set; } = (int)ContactRank.Tambahan;
        }

        public class Method
        {
            public static ExternalParty Load(string? key) => LibExtensions.Load<ExternalParty>(key, "Mitra");

            public static void Simpan(NewRow input, byte[]? logo, string? logoName, string user)
                => ExternalParty.Save(new ExternalParty.NewRow
                {
                    Name = input.Name ?? "",
                    PartyType = (ExternalPartyType)input.PartyType,
                    Industry = input.Industry,
                    Address = input.Address,
                    City = input.City,
                    Phone = input.Phone,
                    Email = input.Email,
                    Website = input.Website,
                    Instagram = input.Instagram,
                    Notes = input.Notes,
                    Logo = logo,
                    LogoFileName = logoName
                }, user).Ok();

            public static void Update(string key, NewRow input, byte[]? logo, string? logoName, bool hapusLogo, string user)
            {
                var obj = Load(key);
                obj.Name = input.Name ?? "";
                obj.PartyType = (ExternalPartyType)input.PartyType;
                obj.Industry = input.Industry;
                obj.Address = input.Address;
                obj.City = input.City;
                obj.Phone = input.Phone;
                obj.Email = input.Email;
                obj.Website = input.Website;
                obj.Instagram = input.Instagram;
                obj.Notes = input.Notes;
                obj.Status = (ExternalPartyStatus)input.Status;
                obj.Update(user).Ok();
                if (logo != null)
                {
                    obj.UpdateLogo(logo, logoName, user).Ok();
                }
                else if (hapusLogo)
                {
                    obj.UpdateLogo(null, null, user).Ok();
                }
            }

            public static void TambahKontak(KontakInput input, string user)
            {
                var obj = Load(input.Key);
                ExternalPartyContact.Save(new ExternalPartyContact.NewRow
                {
                    ExternalPartyId = obj.Id,
                    Name = input.Name ?? "",
                    Position = input.Position,
                    Phone = input.Phone,
                    Email = input.Email,
                    IsPrimary = (ContactRank)input.IsPrimary
                }, user).Ok();
            }

            public static ExternalPartyContact LoadKontak(string? key) => LibExtensions.Load<ExternalPartyContact>(key, "Kontak");

            public static void JadikanUtama(string? key, string user)
            {
                var obj = LoadKontak(key);
                obj.IsPrimary = ContactRank.Utama;
                obj.Update(user).Ok();
            }
        }
    }
}
