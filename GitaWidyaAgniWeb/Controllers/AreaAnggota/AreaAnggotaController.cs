using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Anggota;
using GWAShared.Results;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>
    /// Dasar halaman Area Anggota (/AreaAnggota/&lt;Controller&gt;): cukup login ([LoginOnly]), data selalu milik user yang login.
    /// </summary>
    [Area("AreaAnggota"), LoginOnly]
    public abstract class AreaAnggotaController : BaseController
    {
        private Member? _saya;
        private bool _sayaLoaded;

        /// <summary>Anggota milik user login, atau null bila akun belum terhubung ke anggota.</summary>
        protected Member? SayaAtauNull()
        {
            if (!_sayaLoaded)
            {
                _saya = Member.GetByUser(LibUser);
                _sayaLoaded = true;
            }
            return _saya;
        }

        /// <summary>Anggota milik user login; ArgumentException (pesan ramah) bila belum terhubung.</summary>
        protected Member Saya() => SayaAtauNull() ?? throw new ArgumentException(ExceptionMessage.USER_NOT_MEMBER);
    }
}
