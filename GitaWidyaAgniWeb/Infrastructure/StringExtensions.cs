using System.Text.RegularExpressions;

namespace GitaWidyaAgniWeb.Infrastructure
{
    public static partial class StringExtensions
    {
        // Batas kata PascalCase: "aB" atau "ABc" (akronim tetap utuh: "PengajuanDana" -> "Pengajuan Dana", "EVoting" -> "E Voting").
        [GeneratedRegex("(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])")]
        private static partial Regex PascalCaseBoundary();

        /// <summary>"ProgramKerja" -> "Program Kerja".</summary>
        public static string ToTitleWords(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return PascalCaseBoundary().Replace(value.Trim(), " ");
        }
    }
}
