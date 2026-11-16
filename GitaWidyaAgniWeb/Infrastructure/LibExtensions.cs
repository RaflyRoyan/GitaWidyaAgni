using GWALib.General;
using GWAShared.Enums;
using GWAShared.Results;

namespace GitaWidyaAgniWeb.Infrastructure
{
    public static class LibExtensions
    {
        public const string KEY_EXPIRED = "Data tidak ditemukan atau sesi form sudah kedaluwarsa. Tutup lalu buka ulang form.";

        /// <summary>Data dari Response GWALib; status Error dilempar sebagai ArgumentException (pesan untuk user).</summary>
        public static T Ok<T>(this Response<T> response)
        {
            if (response.Status != ResponseStatus.Success)
            {
                throw new ArgumentException(string.IsNullOrWhiteSpace(response.Message) ? "Proses gagal" : response.Message);
            }
            return response.Data!;
        }

        /// <summary>
        /// Memuat baris lewat konstruktor encKey kelas GWALib. Kunci kosong, tidak valid / kedaluwarsa (encKey berganti tiap hari),
        /// atau baris tidak ada → ArgumentException.
        /// </summary>
        public static T Load<T>(string? encKey, string? nama = null) where T : BaseLib
        {
            if (string.IsNullOrWhiteSpace(encKey))
            {
                throw new ArgumentException(KEY_EXPIRED);
            }
            T? row;
            try
            {
                row = (T?)Activator.CreateInstance(typeof(T), encKey);
            }
            catch
            {
                throw new ArgumentException(KEY_EXPIRED);
            }
            var id = typeof(T).GetProperty("Id")?.GetValue(row);
            if (row == null || (id is int value && value <= 0) || string.IsNullOrEmpty(row.EncDecrKey))
            {
                throw new ArgumentException(nama == null ? KEY_EXPIRED : $"{nama} tidak ditemukan.");
            }
            return row;
        }
    }
}
