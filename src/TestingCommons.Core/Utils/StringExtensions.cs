namespace TestingCommons.Core.Utils;

public static class StringExtensions
{
    /// <summary>
    /// Returns either the source string or alternate string, depending on source. 
    /// </summary>
    /// <param name="source">The source string as the target for the extension.</param>
    /// <param name="alternate">The alternate string to return.</param>
    /// <returns>The alternate string in case the source is blank.</returns>
    public static string IfBlankThen(this string? source, string alternate)
    {
        return string.IsNullOrWhiteSpace(source) ? alternate : source;
    }

    public static int GetOrdinalFromWord(this string? source)
    {
        const string words = "first;second;third;fourth;fifth;sixth;seventh;eighth;ninth;tenth";
        if (string.IsNullOrWhiteSpace(source)) return -1;
        return Array.IndexOf(words.Split(";"), source!.ToLowerInvariant());
    }
}




