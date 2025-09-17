using System.Globalization;
using System.Text;

namespace FZ.Movie.ApplicationService.Service.Implements.Media.Source.Archive
{
    public static class HeaderEncoding
    {
        /// <summary>
        /// Chuyển chuỗi Unicode về US-ASCII cho header: bỏ dấu, thay ký tự ngoài ASCII bằng repl.
        /// Đồng thời loại bỏ control chars (&lt;0x20, 0x7F).
        /// </summary>
        public static string ToAsciiHeader(string? input, char repl = '_')
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var norm = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(norm.Length);

            foreach (var c in norm)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat == UnicodeCategory.NonSpacingMark) continue; // bỏ dấu

                if (c <= 0x7F)
                {
                    // control chars -> thay
                    if (c < 0x20 || c == 0x7F) sb.Append(repl);
                    else sb.Append(c);
                }
                else
                {
                    // thay ký tự ngoài ASCII
                    sb.Append(c switch { 'đ' => 'd', 'Đ' => 'D', _ => repl });
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
