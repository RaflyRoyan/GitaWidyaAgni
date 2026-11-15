using GWAShared.Results;
using SkiaSharp;

namespace GWALib.General.Utilities
{
    public record ImageData(byte[] Data, string ContentType);

    public static class ImageUtility
    {
        public const int MAX_UPLOAD_BYTES = 5 * 1024 * 1024;
        public const int MAX_DIMENSION = 1024;
        public const int JPEG_QUALITY = 85;

        public static ImageData Normalize(byte[]? data)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Foto"));
            }
            if (data.Length > MAX_UPLOAD_BYTES)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FILE_TOO_LARGE, "Foto", MAX_UPLOAD_BYTES / 1024 / 1024));
            }

            using var skData = SKData.CreateCopy(data);
            using var codec = SKCodec.Create(skData)
                ?? throw new ArgumentException(ExceptionMessage.IMAGE_INVALID);
            using var decoded = SKBitmap.Decode(codec)
                ?? throw new ArgumentException(ExceptionMessage.IMAGE_INVALID);

            using var oriented = ApplyOrientation(decoded, codec.EncodedOrigin);
            using var resized = Resize(oriented);

            if (HasTransparency(resized))
            {
                return new ImageData(Encode(resized, SKEncodedImageFormat.Png, 100), "image/png");
            }
            return new ImageData(Encode(resized, SKEncodedImageFormat.Jpeg, JPEG_QUALITY), "image/jpeg");
        }
        public static string GetContentType(byte[] data)
        {
            return data.Length >= 4 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47
                ? "image/png"
                : "image/jpeg";
        }
        private static SKBitmap ApplyOrientation(SKBitmap src, SKEncodedOrigin origin)
        {
            float w = src.Width, h = src.Height;
            SKMatrix matrix;
            switch (origin)
            {
                case SKEncodedOrigin.TopRight: matrix = new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1); break;
                case SKEncodedOrigin.BottomRight: matrix = new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1); break;
                case SKEncodedOrigin.BottomLeft: matrix = new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1); break;
                case SKEncodedOrigin.LeftTop: matrix = new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1); break;
                case SKEncodedOrigin.RightTop: matrix = new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1); break;
                case SKEncodedOrigin.RightBottom: matrix = new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1); break;
                case SKEncodedOrigin.LeftBottom: matrix = new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1); break;
                default: return src.Copy();
            }

            var swap = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
            var dst = new SKBitmap(swap ? src.Height : src.Width, swap ? src.Width : src.Height, src.ColorType, src.AlphaType);
            using var canvas = new SKCanvas(dst);
            canvas.Clear(SKColors.Transparent);
            canvas.SetMatrix(matrix);
            using var image = SKImage.FromBitmap(src);
            canvas.DrawImage(image, 0, 0, new SKSamplingOptions(SKFilterMode.Linear));
            return dst;
        }
        private static SKBitmap Resize(SKBitmap src)
        {
            var longest = Math.Max(src.Width, src.Height);
            if (longest <= MAX_DIMENSION)
            {
                return src.Copy();
            }
            var scale = (double)MAX_DIMENSION / longest;
            var info = new SKImageInfo(Math.Max(1, (int)Math.Round(src.Width * scale)), Math.Max(1, (int)Math.Round(src.Height * scale)),
                src.ColorType, src.AlphaType);
            return src.Resize(info, new SKSamplingOptions(SKCubicResampler.Mitchell))
                ?? throw new ArgumentException(ExceptionMessage.IMAGE_PROCESS_FAILED);
        }
        private static bool HasTransparency(SKBitmap bitmap)
        {
            if (bitmap.AlphaType == SKAlphaType.Opaque)
            {
                return false;
            }
            return bitmap.Pixels.Any(x => x.Alpha < 255);
        }
        private static byte[] Encode(SKBitmap bitmap, SKEncodedImageFormat format, int quality)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var encoded = image.Encode(format, quality)
                ?? throw new ArgumentException(ExceptionMessage.IMAGE_PROCESS_FAILED);
            return encoded.ToArray();
        }
    }
}
