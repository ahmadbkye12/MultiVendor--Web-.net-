namespace Application.Common;

public static class SlugHelper
{
    public static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "item";

        Span<char> buffer = stackalloc char[value.Length];
        var bi = 0;
        var lastHyphen = false;

        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            var ok = char.IsAsciiLetterLower(ch) || char.IsAsciiDigit(ch);
            if (ok)
            {
                buffer[bi++] = ch;
                lastHyphen = false;
                continue;
            }

            if (ch is ' ' or '-' or '_')
            {
                if (!lastHyphen && bi > 0)
                {
                    buffer[bi++] = '-';
                    lastHyphen = true;
                }
            }
        }

        while (bi > 0 && buffer[bi - 1] == '-')
            bi--;

        return bi == 0 ? "item" : new string(buffer[..bi]);
    }
}
