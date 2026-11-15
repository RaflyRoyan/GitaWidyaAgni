using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWAShared.Extensions
{
    public static class DecimalExtensions
    {
        private static readonly string[] units = { "", "satu", "dua", "tiga", "empat", "lima", "enam", "tujuh", "delapan", "sembilan", "sepuluh", "sebelas", "dua belas", "tiga belas", "empat belas", "lima belas", "enam belas", "tujuh belas", "delapan belas", "sembilan belas" };
        private static readonly string[] tens = { "", "sepuluh", "dua puluh", "tiga puluh", "empat puluh", "lima puluh", "enam puluh", "tujuh puluh", "delapan puluh", "sembilan puluh" };
        private static readonly string[] scales = { "", "ribu", "juta", "miliar", "triliun", "quadriliun", "quintiliun", "sekstiliun", "septiliun", "oktiliun", "noniliun", "desiliun" };

        public static string FormatRupiah(decimal nominal)
        {
            if (nominal >= 1_000_000_000_000)
                return $"Rp. {nominal / 1_000_000_000_000M:N3} T";
            else if (nominal >= 1_000_000_000)
                return $"Rp. {nominal / 1_000_000_000M:N2} M";
            else if (nominal >= 1_000_000)
                return $"Rp. {nominal / 1_000_000M:N1} Juta";
            else if (nominal >= 1_000)
                return $"Rp. {nominal / 1_000M:N0} Ribu";
            else
                return $"Rp. {nominal:N0}";
        }
        public static string FormatRupiahFull(decimal nominal)
        {
            return $"Rp. {nominal.ToString("n0")}";
        }
        public static string ConvertMoneyToWordsID(long number)
        {
            if (number == 0)
            {
                return "nol";
            }

            string words = "";

            int scaleIndex = 0;
            while (number > 0)
            {
                int currentScaleValue = (int)(number % 1000);

                if (currentScaleValue != 0)
                {
                    string scaleWords = ConvertThreeDigitNumberToWords(currentScaleValue);
                    words = scaleWords + " " + scales[scaleIndex] + " " + words;
                }

                number /= 1000;
                scaleIndex++;
            }
            return words.Trim().ToUpper();
        }
        private static string ConvertThreeDigitNumberToWords(int number)
        {
            string words = "";

            if (number < 20)
            {
                words = units[number];
            }
            else if (number < 100)
            {
                words = tens[number / 10] + " " + units[number % 10];
            }
            else
            {
                words = (number == 1000 ? "seribu" : (number / 100 == 1 ? "seratus" : units[number / 100] + " ratus")) + " " + ConvertThreeDigitNumberToWords(number % 100);
            }

            return words;
        }
    }
}
