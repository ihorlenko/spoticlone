using System.Text.RegularExpressions;
using SpotiClone.Models;

namespace SpotiClone.Helpers;

public static class LrcParser
{
    // matches [mm:ss.xx] or [mm:ss.xxx]
    private static readonly Regex TimeTag = new(@"^\[(\d{1,2}):(\d{2})\.(\d{2,3})\](.*)$");

    public static List<LyricLine> Parse(string lrc)
    {
        var lines = new List<LyricLine>();
        foreach (var raw in lrc.Split('\n'))
        {
            var m = TimeTag.Match(raw.Trim());
            if (!m.Success) continue;

            var minutes = int.Parse(m.Groups[1].Value);
            var seconds = int.Parse(m.Groups[2].Value);
            var frac = m.Groups[3].Value;
            // centiseconds (2 digits) → ×10ms; milliseconds (3 digits) → as-is
            var fracMs = frac.Length == 2 ? int.Parse(frac) * 10 : int.Parse(frac);

            var text = m.Groups[4].Value.Trim();
            if (string.IsNullOrEmpty(text)) continue;

            lines.Add(new LyricLine
            {
                TimeMs = (minutes * 60 + seconds) * 1000 + fracMs,
                Text = text
            });
        }
        lines.Sort((a, b) => a.TimeMs.CompareTo(b.TimeMs));
        return lines;
    }
}
