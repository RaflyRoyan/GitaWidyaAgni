using GWALib.Lib.Transaksi.Approval;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi
{
    internal static class TransaksiHelper
    {
        internal static void ValidateStatus<TEnum>(TEnum current, string action, params TEnum[] allowed) where TEnum : struct, Enum
        {
            if (!allowed.Contains(current))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.STATUS_ACTION_INVALID, action, current.GetDescription()));
            }
        }
        // Teks bebas dipotong sesuai panjang kolom (pesan error / log job bisa lebih panjang dari kolomnya).
        internal static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            var text = value.Trim();
            return text.Length <= maxLength ? text : text[..maxLength];
        }
        internal static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        // Menambahkan catatan ke kolom teks yang sudah ada (mis. alasan batal / tolak), dipotong sesuai panjang kolom.
        internal static string? AppendNote(string? current, string label, string? note, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                return current;
            }
            var text = label + ": " + note.Trim();
            return Truncate(string.IsNullOrWhiteSpace(current) ? text : current + " | " + text, maxLength);
        }
        // Nomor dokumen dari DocumentNumberSeq bila format jenis dokumen aktif; kalau belum diatur dipakai PREFIX/yyyyMMdd/id.
        // Panggil di dalam BeginTransaction supaya nomor ikut di-rollback bila dokumen gagal disimpan.
        internal static string GenerateNumber(EF.ModelContext context, string documentType, DateTime date, string fallbackPrefix, int id, int? userId)
        {
            if (context.MDocumentNumberFormats.Any(x => x.DocumentType == documentType && x.IsActive == (int)ActiveStatus.Aktif))
            {
                return DocumentNumberSeq.GenerateNumber(context, documentType, date, userId);
            }
            return $"{fallbackPrefix}/{date:yyyyMMdd}/{id:D6}";
        }
        // File wajib (tidak boleh kosong) dengan batas ukuran; mengembalikan nama file tanpa path.
        internal static string ValidateFile(byte[]? file, string? fileName, string label, int maxBytes)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, label));
            }
            if (file.Length > maxBytes)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FILE_TOO_LARGE, label, maxBytes / 1024 / 1024));
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama " + label));
            }
            return Path.GetFileName(fileName.Trim());
        }
        internal static void ValidatePositive(decimal value, string label)
        {
            if (value <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.AMOUNT_MUST_POSITIVE, label));
            }
        }
    }
}
