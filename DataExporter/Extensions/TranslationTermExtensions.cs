using I2.Loc;
using Utils;

namespace DataExporter.Extensions
{
    public static class TranslationTermExtensions
    {
        public static string ToEnglish(this TranslationTerm term)
        {
            return !term.IsEmpty() ? LocalizationManager.GetTermTranslation(term.Term, overrideLanguage: "English") : term.PlaceHolderString;
        }

        public static bool IsEmpty(this TranslationTerm term)
        {
            return string.IsNullOrWhiteSpace(term?.Term) || term.Term == "No Text";
        }
    }
}
