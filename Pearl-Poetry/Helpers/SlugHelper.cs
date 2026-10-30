using System.Text;
using System.Text.RegularExpressions;

namespace Pearl_Poetry.Helpers
{
    public static class SlugHelper
    {
        public static string Generate(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            text = text.Trim().ToLowerInvariant();

            // Remove accents/diacritics
            text = RemoveDiacritics(text);

            // Replace anything that isn't a letter or number
            // with a hyphen
            text = Regex.Replace(
                text,
                @"[^a-z0-9]+",
                "-"
            );

            // Remove hyphens from beginning/end
            text = text.Trim('-');

            return text;
        }


        private static string RemoveDiacritics(string text)
        {
            var normalizedString =
                text.Normalize(NormalizationForm.FormD);

            var stringBuilder =
                new StringBuilder();

            foreach (char character in normalizedString)
            {
                var unicodeCategory =
                    System.Globalization.CharUnicodeInfo
                        .GetUnicodeCategory(character);

                if (unicodeCategory !=
                    System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(character);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }
    }
}