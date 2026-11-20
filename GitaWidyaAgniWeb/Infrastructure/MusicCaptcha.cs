using Microsoft.Extensions.Caching.Memory;
using SkiaSharp;
using System.Security.Cryptography;
using System.Text;

namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>
    /// Captcha login "seret ikon yang sama (bentuk &amp; arah)" — pola IconCaptcha BEGateSecuritySystemWeb, dengan ikon bertema
    /// marching band yang digambar sendiri (not balok, trompet, bendera colour guard, stik drum, tongkat mayoret, dst.).
    /// Target dan pilihan digambar di server sebagai PNG (rotasi, warna, posisi, noise acak) dengan ID pilihan acak; ikon yang benar
    /// tidak pernah dikirim ke browser. Browser mengirim ID ikon yang dijatuhkan ke target, server memverifikasi lalu menerbitkan token
    /// sekali pakai yang dicek saat POST login. Jawaban dan token di IMemoryCache (singleton; satu instance web atau sticky session).
    /// </summary>
    public class MusicCaptcha(IMemoryCache cache)
    {
        public const int JUMLAH_PILIHAN = 6;
        public const int UKURAN_TARGET = 64;
        public const int UKURAN_PILIHAN = 52;
        private const int SKALA_GAMBAR = 2; // digambar 2x agar tajam di layar HiDPI
        private const int MIN_DURASI_MS = 100;
        private const int JUMLAH_KEMBARAN = 3; // bentuk jawaban muncul 3x dengan arah berbeda; hanya 1 yang arahnya sama dengan target
        private static readonly TimeSpan BERLAKU_PUZZLE = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan BERLAKU_TOKEN = TimeSpan.FromMinutes(10);
        private const string CACHE_PUZZLE = "musiccaptcha:puzzle:";
        private const string CACHE_TOKEN = "musiccaptcha:token:";

        // Ikon musik (koordinat 24x24), semuanya berarah: diputar 90° / 180° / 270° terlihat berbeda.
        private static readonly Func<SKPath>[] IKON =
        {
            // 0 not seperempat: kepala not miring + batang
            () => Gabung(Oval(8.6f, 17.6f, 3.7f, 2.6f, -22), Kotak(11.4f, 3.5f, 1.9f, 14.2f)),
            // 1 not seperdelapan: not seperempat + bendera
            () => Gabung(Oval(8.6f, 17.6f, 3.7f, 2.6f, -22), Kotak(11.4f, 3.5f, 1.9f, 14.2f),
                Svg("M13.3 3.5c.4 3.1 5.6 4.3 5.6 9 0 1.2-.4 2.3-1 3.2.2-.8.3-1.5.3-2.2 0-3.1-3.4-4.6-4.9-5.2Z")),
            // 2 dua not seperdelapan berbalok
            () => Gabung(Oval(6.2f, 18.4f, 3.1f, 2.2f, -22), Oval(16.2f, 15.9f, 3.1f, 2.2f, -22),
                Kotak(8.4f, 7.2f, 1.7f, 11.3f), Kotak(18.4f, 4.6f, 1.7f, 11.4f), Svg("M8.4 6.4L20.1 3.4v2.9L8.4 9.3Z")),
            // 3 trompet: corong mulut, pipa, 3 piston, corong suara
            () => Gabung(Kotak(1.5f, 11f, 3f, 2f), Kotak(4.5f, 10.4f, 9.5f, 3.2f), Svg("M14 10.4L22 6v12l-8-4.4Z"),
                Kotak(6.6f, 6.5f, 1.5f, 4f), Kotak(9.1f, 6.5f, 1.5f, 4f), Kotak(11.6f, 6.5f, 1.5f, 4f)),
            // 4 bendera colour guard: tiang + kain berkibar
            () => Gabung(Kotak(4f, 2f, 2f, 20f), Svg("M6 3c4.2-2.2 8.2 2.2 13.5 0v9.5c-5.3 2.2-9.3-2.2-13.5 0Z")),
            // 5 mikrofon dengan dudukan
            () => Gabung(Svg("M9 2.5h6v9a3 3 0 0 1-6 0Z"),
                Svg("M5 10.5h2a5 5 0 0 0 10 0h2a7 7 0 0 1-6 6.9V20h3v2H8v-2h3v-2.6A7 7 0 0 1 5 10.5Z")),
            // 6 stik drum: batang miring + ujung bulat
            () => Gabung(Svg("M3.2 19.4L15.6 7l1.6 1.6L4.8 21Z"), Lingkaran(18.2f, 5.8f, 3.2f)),
            // 7 speaker dengan gelombang suara
            () => Gabung(Svg("M2.5 9h4.2L12 4v16l-5.3-5H2.5Z"), Svg("M14.6 8.4a5 5 0 0 1 0 7.2l-1.4-1.4a3 3 0 0 0 0-4.4Z"),
                Svg("M17.4 5.6a9 9 0 0 1 0 12.8L16 17a7 7 0 0 0 0-10Z")),
            // 8 headphone
            () => Gabung(Svg("M3.5 13a8.5 8.5 0 0 1 17 0h-2.2a6.3 6.3 0 0 0-12.6 0Z"), Kotak(2.5f, 12.5f, 4.5f, 8.5f), Kotak(17f, 12.5f, 4.5f, 8.5f)),
            // 9 tongkat mayoret: bola besar di atas, batang, bola kecil di bawah
            () => Gabung(Lingkaran(12f, 4.6f, 3.2f), Kotak(11f, 6.5f, 2f, 13.5f), Lingkaran(12f, 20.6f, 1.9f)),
        };

        // Pasangan yang terlalu mirip: bila salah satu jadi jawaban, yang lain tidak dipakai sebagai pengecoh.
        private static readonly int[][] MIRIP = { new[] { 0, 1 }, new[] { 1, 2 } };

        public class Pilihan
        {
            public string Id { get; set; } = "";
            public string Gambar { get; set; } = ""; // data:image/png;base64
        }

        public class Puzzle
        {
            public string PuzzleId { get; set; } = "";
            public string Target { get; set; } = "";
            public List<Pilihan> Pilihan { get; set; } = new();
        }

        public Puzzle Buat()
        {
            var bentukBenar = Acak(0, IKON.Length);
            var arah = Enumerable.Range(0, 4).OrderBy(_ => Acak(0, int.MaxValue)).Take(JUMLAH_KEMBARAN).ToArray(); // arah[0] = jawaban
            var mirip = MIRIP.Where(m => m.Contains(bentukBenar)).SelectMany(m => m).ToHashSet();

            var daftar = arah.Select((a, i) => (Bentuk: bentukBenar, Arah: a, Benar: i == 0)).ToList();
            daftar.AddRange(Enumerable.Range(0, IKON.Length).Where(i => i != bentukBenar && !mirip.Contains(i))
                .OrderBy(_ => Acak(0, int.MaxValue)).Take(JUMLAH_PILIHAN - JUMLAH_KEMBARAN)
                .Select(i => (Bentuk: i, Arah: Acak(0, 4), Benar: false)));
            daftar = daftar.OrderBy(_ => Acak(0, int.MaxValue)).ToList();

            var puzzle = new Puzzle { PuzzleId = AcakHex(16), Target = KeDataUrl(GambarTarget(bentukBenar, arah[0])) };
            var jawaban = "";
            foreach (var item in daftar)
            {
                var id = AcakHex(8);
                if (item.Benar)
                {
                    jawaban = id;
                }
                puzzle.Pilihan.Add(new Pilihan { Id = id, Gambar = KeDataUrl(GambarPilihan(item.Bentuk, item.Arah)) });
            }
            cache.Set(CACHE_PUZZLE + puzzle.PuzzleId, jawaban, BERLAKU_PUZZLE);
            return puzzle;
        }

        /// <summary>(berhasil, token). Setiap puzzle hanya bisa dicoba sekali; gagal = browser memuat puzzle baru.</summary>
        public (bool Berhasil, string? Token) Verifikasi(string? puzzleId, string? pilihan, int durasiMs)
        {
            if (string.IsNullOrEmpty(puzzleId) || string.IsNullOrEmpty(pilihan))
            {
                return (false, null);
            }
            var key = CACHE_PUZZLE + puzzleId;
            if (!cache.TryGetValue(key, out string? jawaban) || string.IsNullOrEmpty(jawaban))
            {
                return (false, null);
            }
            cache.Remove(key); // benar maupun salah, puzzle hangus
            var cocok = durasiMs >= MIN_DURASI_MS
                && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(pilihan), Encoding.UTF8.GetBytes(jawaban));
            if (!cocok)
            {
                return (false, null);
            }
            var token = AcakHex(16);
            cache.Set(CACHE_TOKEN + token, true, BERLAKU_TOKEN);
            return (true, token);
        }

        /// <summary>Dicek saat POST login; token langsung hangus.</summary>
        public bool CekToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            var key = CACHE_TOKEN + token;
            if (!cache.TryGetValue(key, out _))
            {
                return false;
            }
            cache.Remove(key);
            return true;
        }

        // ===================== BENTUK =====================
        private static SKPath Svg(string data) => SKPath.ParseSvgPathData(data);

        private static SKPath Kotak(float x, float y, float w, float h)
        {
            var p = new SKPath();
            p.AddRect(new SKRect(x, y, x + w, y + h));
            return p;
        }

        private static SKPath Lingkaran(float cx, float cy, float r)
        {
            var p = new SKPath();
            p.AddCircle(cx, cy, r);
            return p;
        }

        private static SKPath Oval(float cx, float cy, float rx, float ry, float derajat)
        {
            var p = new SKPath();
            p.AddOval(new SKRect(cx - rx, cy - ry, cx + rx, cy + ry));
            p.Transform(SKMatrix.CreateRotationDegrees(derajat, cx, cy));
            return p;
        }

        // Gabungkan bagian ikon (union), jadi bagian yang tumpang tindih tidak berlubang.
        private static SKPath Gabung(params SKPath[] bagian)
        {
            var hasil = new SKPath();
            foreach (var b in bagian)
            {
                using (b)
                {
                    var u = hasil.Op(b, SKPathOp.Union) ?? hasil;
                    if (!ReferenceEquals(u, hasil))
                    {
                        hasil.Dispose();
                        hasil = u;
                    }
                }
            }
            return hasil;
        }

        // ===================== GAMBAR =====================
        private static int Acak(int min, int max) => RandomNumberGenerator.GetInt32(min, max);
        private static string AcakHex(int jumlahByte) => Convert.ToHexString(RandomNumberGenerator.GetBytes(jumlahByte));

        // arah 0..3 = 0°/90°/180°/270°, ditambah goyangan kecil agar gambar tidak identik piksel per piksel
        private static float Rotasi(int arah) => arah * 90 + Acak(-8, 9);

        // Target: ikon perunggu dengan pendar emas (tema GWA) di atas latar krem.
        private static SKImage GambarTarget(int bentuk, int arah)
        {
            var s = UKURAN_TARGET * SKALA_GAMBAR;
            using var surface = SKSurface.Create(new SKImageInfo(s, s));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            GambarNoise(canvas, s, 10);
            using var path = Posisikan(IKON[bentuk](), s, Rotasi(arah), Acak(58, 68) / 100f);
            using var pendar = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(232, 181, 42),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3 * SKALA_GAMBAR,
                StrokeJoin = SKStrokeJoin.Round,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 1.5f * SKALA_GAMBAR)
            };
            using var isi = new SKPaint { IsAntialias = true, Color = new SKColor(154, 107, 0) };
            canvas.DrawPath(path, pendar);
            canvas.DrawPath(path, isi);
            return surface.Snapshot();
        }

        // Pilihan: warna acak (cukup gelap di atas putih), ukuran sedikit acak.
        private static SKImage GambarPilihan(int bentuk, int arah)
        {
            var s = UKURAN_PILIHAN * SKALA_GAMBAR;
            using var surface = SKSurface.Create(new SKImageInfo(s, s));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            GambarNoise(canvas, s, 8);
            using var path = Posisikan(IKON[bentuk](), s, Rotasi(arah), Acak(62, 74) / 100f);
            using var isi = new SKPaint { IsAntialias = true, Color = SKColor.FromHsl(Acak(0, 360), Acak(35, 70), Acak(26, 42)) };
            canvas.DrawPath(path, isi);
            return surface.Snapshot();
        }

        private static SKPath Posisikan(SKPath path, int ukuranKanvas, float rotasi, float porsi)
        {
            var skala = ukuranKanvas * porsi / 24f;
            var geser = ukuranKanvas * 0.04f;
            var matrix = SKMatrix.CreateTranslation(-12, -12)
                .PostConcat(SKMatrix.CreateScale(skala, skala))
                .PostConcat(SKMatrix.CreateRotationDegrees(rotasi))
                .PostConcat(SKMatrix.CreateTranslation(ukuranKanvas / 2f + Acak(-1, 2) * geser, ukuranKanvas / 2f + Acak(-1, 2) * geser));
            path.Transform(matrix);
            return path;
        }

        // Titik dan garis tipis agar gambar tidak mudah dicocokkan otomatis.
        private static void GambarNoise(SKCanvas canvas, int s, int jumlah)
        {
            using var paint = new SKPaint { IsAntialias = true, StrokeWidth = SKALA_GAMBAR };
            for (var i = 0; i < jumlah; i++)
            {
                paint.Color = SKColor.FromHsl(Acak(0, 360), Acak(30, 70), Acak(40, 70), (byte)Acak(40, 110));
                if (i % 2 == 0)
                {
                    canvas.DrawCircle(Acak(0, s), Acak(0, s), Acak(1, 3) * SKALA_GAMBAR, paint);
                }
                else
                {
                    canvas.DrawLine(Acak(0, s), Acak(0, s), Acak(0, s), Acak(0, s), paint);
                }
            }
        }

        private static string KeDataUrl(SKImage image)
        {
            using (image)
            using (var data = image.Encode(SKEncodedImageFormat.Png, 90))
            {
                return "data:image/png;base64," + Convert.ToBase64String(data.ToArray());
            }
        }
    }
}
