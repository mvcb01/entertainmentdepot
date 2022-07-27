using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using FilmDomain.Entities;

namespace FilmDomain.Extensions
{
    public static class EntityExtensions
    {
        public static IEnumerable<string> GetFileNames(this IEnumerable<MovieRip> movieRips)
        {
            return movieRips.Select(r => r.FileName);
        }

        public static IEnumerable<DateTime> GetVisitDates(this IEnumerable<MovieWarehouseVisit> visits)
        {
            return visits.Select(v => v.VisitDateTime);
        }

        public static IEnumerable<string> GetStringTokensWithoutPunctuation(this string value, bool removeDiacritics = true)
        {
            if (value == null)
            {
                return Enumerable.Empty<string>();
            }

            value = value.Trim().ToLower();

            if (removeDiacritics)
            {
                value = RemoveDiacritics(value);
            }

            IEnumerable<Char> charsToRemove = value.Where(c => !Char.IsLetterOrDigit(c)).Distinct();
            return value.Split().Select(s => s.Trim(charsToRemove.ToArray())).Where(s => !string.IsNullOrEmpty(s));
        }

        public static string WithoutDiacritics(this string value)
        {
            return RemoveDiacritics(value);
        }

        // taken from
        // https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net/249126#249126
        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString.EnumerateRunes())
            {
                var unicodeCategory = Rune.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}