using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.AreaAnggota
{
    /// <summary>Area Anggota › Profil Saya: data diri (kontak bisa diubah sendiri), foto, jabatan, divisi, dan ganti password.</summary>
    public class ProfilSayaVM
    {
        public class Index
        {
            public string Username { get; set; } = "";
            public Member? Data { get; set; }
            public bool AdaFoto { get; set; }
            public NewKontak Kontak { get; set; } = new();
            public List<string> Jabatan { get; set; } = new();
            public List<string> Divisi { get; set; } = new();

            public Index(string username, Member? member)
            {
                Username = username;
                Data = member;
                if (member == null)
                {
                    return;
                }
                AdaFoto = MemberPhoto.GetInfoList(member.Id).Count > 0;
                Kontak = new NewKontak { Phone = member.Phone, Email = member.Email, Address = member.Address };
                Jabatan = ManagementAssignment.GetDataList(memberId: member.Id)
                    .OrderByDescending(x => x.StartDate)
                    .Select(x => x.PositionName + (x.DivisionName != null ? " " + x.DivisionName : "") + " · " + x.PeriodName
                        + (x.IsCurrent == AssignmentPeriod.Berjalan ? "" : " (selesai)"))
                    .ToList();
                Divisi = DivisionMember.GetDataList(FilterAktif.Aktif, memberId: member.Id)
                    .Select(x => x.DivisionName + (string.IsNullOrEmpty(x.MemberRole) ? "" : " · " + x.MemberRole))
                    .ToList();
            }
        }

        public class NewKontak
        {
            [StringLength(30, ErrorMessage = "No. HP maksimal 30 karakter")]
            [Display(Name = "No. HP / WhatsApp")]
            public string? Phone { get; set; }

            [EmailAddress(ErrorMessage = "Format email tidak valid")]
            [StringLength(100, ErrorMessage = "Email maksimal 100 karakter")]
            [Display(Name = "Email")]
            public string? Email { get; set; }

            [StringLength(255, ErrorMessage = "Alamat maksimal 255 karakter")]
            [Display(Name = "Alamat")]
            public string? Address { get; set; }
        }

        public class PasswordInput
        {
            [Required(ErrorMessage = "Password lama harus diisi")]
            public string? Lama { get; set; }

            [Required(ErrorMessage = "Password baru harus diisi")]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Password baru minimal 8 karakter")]
            public string? Baru { get; set; }

            [Required(ErrorMessage = "Ulangi password baru")]
            [Compare(nameof(Baru), ErrorMessage = "Ulangan password baru tidak sama")]
            public string? Ulang { get; set; }
        }
    }
}
