using GWALib.Lib.Transaksi.Approval;

namespace GWALib.Lib.Transaksi.Information
{
    /// <summary>
    /// Notifikasi in-app dari modul lain, di dalam context pemanggil (ikut tersimpan bersama perubahan, tanpa SaveChanges).
    /// Notifikasi bersifat pelengkap: kegagalan validasinya ditelan supaya tidak membatalkan perubahan utama.
    /// Pengirim (insBy) tidak menerima notifikasi atas tindakannya sendiri.
    /// Link berisi path halaman web GitaWidyaAgniWeb.
    /// </summary>
    internal static class NotifyHelper
    {
        // Jenis notifikasi (kode m_user_notification_pref / m_message_template).
        internal const string TYPE_APPROVAL_REQUEST = "PERSETUJUAN";
        internal const string TYPE_APPROVAL_RESULT = "HASIL_PERSETUJUAN";
        internal const string TYPE_DISPOSITION = "DISPOSISI";
        internal const string TYPE_DUES = "TAGIHAN_IURAN";
        internal const string TYPE_ELECTION = "PEMILIHAN";
        internal const string TYPE_COMMITTEE = "TIM_PELAKSANA";
        internal const string TYPE_ORDER = "PESANAN";
        internal const string TYPE_BORROW = "PEMINJAMAN";
        internal const string TYPE_CERTIFICATE = "SERTIFIKAT";
        internal const string TYPE_ASPIRATION = "ASPIRASI";
        internal const string TYPE_NEWS = "BERITA";
        internal const string TYPE_ASSIGNMENT = "KEPENGURUSAN";
        internal const string TYPE_ABSENCE = "IZIN_KEGIATAN";
        internal const string TYPE_ATTENDANCE = "PRESENSI";

        // Halaman web tujuan.
        internal const string LINK_LETTER = "/Persuratan/Surat";
        internal const string LINK_FUNDING = "/Keuangan/PengajuanDana";
        internal const string LINK_PROCUREMENT = "/Pengadaan/Pengadaan";
        internal const string LINK_MY_DISPOSITION = "/AreaAnggota/DisposisiSaya";
        internal const string LINK_MY_DUES = "/AreaAnggota/IuranSaya";
        internal const string LINK_MY_ELECTION = "/AreaAnggota/Pemilihan";
        internal const string LINK_MY_ACTIVITY = "/AreaAnggota/KegiatanSaya";
        internal const string LINK_MY_ORDER = "/AreaAnggota/Toko";
        internal const string LINK_MY_BORROW = "/AreaAnggota/PeminjamanSaya";
        internal const string LINK_MY_ACHIEVEMENT = "/AreaAnggota/PrestasiSaya";
        internal const string LINK_MY_FORM = "/AreaAnggota/FormulirSaya";
        internal const string LINK_MY_PROFILE = "/AreaAnggota/ProfilSaya";
        internal const string LINK_MY_ATTENDANCE = "/AreaAnggota/PresensiSaya";
        internal const string LINK_NEWS = "/berita/";

        internal static int ToUsers(EF.ModelContext context, IEnumerable<int?> userIds, string type, string title, string? message,
            string? link, string refType, int refId, int? insBy)
        {
            var ids = userIds.Where(x => x > 0 && x != insBy).Select(x => x!.Value).Distinct().ToList();
            if (ids.Count == 0)
            {
                return 0;
            }
            try
            {
                return Notification.AddMany(context, new Notification.NewRow
                {
                    UserIds = ids,
                    NotifType = type,
                    Title = TransaksiHelper.Truncate(title, 200) ?? title,
                    Message = TransaksiHelper.Truncate(message, 500),
                    LinkUrl = link,
                    RefType = refType,
                    RefId = refId
                }, insBy);
            }
            catch (ArgumentException)
            {
                return 0;
            }
        }
        internal static int ToUser(EF.ModelContext context, int? userId, string type, string title, string? message,
            string? link, string refType, int refId, int? insBy)
        {
            return ToUsers(context, new[] { userId }, type, title, message, link, refType, refId, insBy);
        }
        // Anggota tanpa akun user dilewati.
        internal static int ToMembers(EF.ModelContext context, IEnumerable<int?> memberIds, string type, string title, string? message,
            string? link, string refType, int refId, int? insBy)
        {
            var ids = memberIds.Where(x => x > 0).Select(x => x!.Value).Distinct().ToList();
            if (ids.Count == 0)
            {
                return 0;
            }
            var users = context.MMembers.Where(x => ids.Contains(x.Id) && x.UserId != null).Select(x => x.UserId).ToList();
            return ToUsers(context, users, type, title, message, link, refType, refId, insBy);
        }
        internal static int ToMember(EF.ModelContext context, int? memberId, string type, string title, string? message,
            string? link, string refType, int refId, int? insBy)
        {
            return ToMembers(context, new[] { memberId }, type, title, message, link, refType, refId, insBy);
        }
        // Pemutus langkah persetujuan berikutnya.
        internal static int ToApprovers(EF.ModelContext context, EF.MApprovalFlowStep step, string title, string? message,
            string link, string refType, int refId, int? insBy)
        {
            var users = ApprovalHelper.GetApproverUserIds(context, step).Select(x => (int?)x);
            return ToUsers(context, users, TYPE_APPROVAL_REQUEST, title, message, link, refType, refId, insBy);
        }
    }
}
