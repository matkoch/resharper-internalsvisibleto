using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.Util;

namespace InternalsVisibleTo.ReSharper
{
    public partial class InternalsVisibleToSuggestionRule
    {
        private static TextLookupRanges CreateTextLookupRanges(TextRange replaceRange)
        {
            return new TextLookupRanges(replaceRange, false, replaceRange);
        }
    }
}