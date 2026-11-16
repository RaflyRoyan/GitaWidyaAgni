using GWAShared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.EVoting
{
    /// <summary>
    /// Enkripsi suara e-voting (AES-256-GCM). Kunci induk 32 byte diberikan aplikasi saat startup (ElectionVote.ConfigureKey) dari
    /// secret / konfigurasi, tidak pernah disimpan di database. Kunci per pemilihan diturunkan dengan HKDF-SHA256 (info = election id),
    /// dan AAD = election id + ballot reference sehingga ciphertext tidak bisa dipindah ke baris / pemilihan lain.
    /// Format encrypted_choice: "v1:" + base64(nonce 12 byte | ciphertext | tag 16 byte). Plaintext: "{candidateId}|{acak}".
    /// </summary>
    internal static class ElectionCrypto
    {
        private const string VERSION = "v1:";
        private const int NONCE_BYTES = 12;
        private const int TAG_BYTES = 16;
        private static byte[]? _masterKey;

        internal static void Configure(string base64Key)
        {
            byte[] key;
            try
            {
                key = Convert.FromBase64String(base64Key?.Trim() ?? string.Empty);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Kunci e-voting harus base64 dari 32 byte acak");
            }
            if (key.Length != 32)
            {
                throw new ArgumentException("Kunci e-voting harus base64 dari 32 byte acak");
            }
            _masterKey = key;
        }
        internal static bool IsConfigured => _masterKey != null;

        internal static void EnsureConfigured()
        {
            if (_masterKey == null)
            {
                throw new ArgumentException(ExceptionMessage.ELECTION_KEY_NOT_SET);
            }
        }
        internal static string Encrypt(int electionId, string ballotReference, int candidateId)
        {
            var key = DeriveKey(electionId);
            var plain = Encoding.UTF8.GetBytes($"{candidateId}|{Convert.ToHexString(RandomNumberGenerator.GetBytes(16))}");
            var nonce = RandomNumberGenerator.GetBytes(NONCE_BYTES);
            var cipher = new byte[plain.Length];
            var tag = new byte[TAG_BYTES];
            using (var aes = new AesGcm(key, TAG_BYTES))
            {
                aes.Encrypt(nonce, plain, cipher, tag, Aad(electionId, ballotReference));
            }
            CryptographicOperations.ZeroMemory(key);
            return VERSION + Convert.ToBase64String(nonce.Concat(cipher).Concat(tag).ToArray());
        }
        // Null bila ciphertext rusak / kunci salah / format tidak dikenal.
        internal static int? Decrypt(int electionId, string ballotReference, string encrypted)
        {
            if (string.IsNullOrEmpty(encrypted) || !encrypted.StartsWith(VERSION, StringComparison.Ordinal))
            {
                return null;
            }
            byte[] data;
            try
            {
                data = Convert.FromBase64String(encrypted[VERSION.Length..]);
            }
            catch (FormatException)
            {
                return null;
            }
            if (data.Length <= NONCE_BYTES + TAG_BYTES)
            {
                return null;
            }
            var key = DeriveKey(electionId);
            try
            {
                var nonce = data.AsSpan(0, NONCE_BYTES);
                var cipher = data.AsSpan(NONCE_BYTES, data.Length - NONCE_BYTES - TAG_BYTES);
                var tag = data.AsSpan(data.Length - TAG_BYTES);
                var plain = new byte[cipher.Length];
                using var aes = new AesGcm(key, TAG_BYTES);
                aes.Decrypt(nonce, cipher, tag, plain, Aad(electionId, ballotReference));
                var text = Encoding.UTF8.GetString(plain);
                var sep = text.IndexOf('|');
                return sep > 0 && int.TryParse(text[..sep], out var candidateId) ? candidateId : null;
            }
            catch (CryptographicException)
            {
                return null;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(key);
            }
        }
        // Token tanda terima untuk pemilih (256-bit, base64url) dan hash SHA-256 hex (64 karakter) yang disimpan sebagai ballot_reference.
        internal static (string Receipt, string Reference) NewBallot()
        {
            var receipt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return (receipt, HashReceipt(receipt));
        }
        internal static string HashReceipt(string receipt)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(receipt.Trim()))).ToLowerInvariant();
        }
        // Id acak t_election_vote (bukan m_sequence) supaya urutan memilih tidak terbaca.
        internal static int NewVoteId()
        {
            return RandomNumberGenerator.GetInt32(1, int.MaxValue);
        }
        private static byte[] DeriveKey(int electionId)
        {
            EnsureConfigured();
            return HKDF.DeriveKey(HashAlgorithmName.SHA256, _masterKey!, 32, info: Encoding.UTF8.GetBytes($"gwa-election:{electionId}"));
        }
        private static byte[] Aad(int electionId, string ballotReference)
        {
            return Encoding.UTF8.GetBytes($"{electionId}|{ballotReference}");
        }
    }
}
