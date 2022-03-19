using System.Text.RegularExpressions;

public class Utils {
public static string? RegexOut(string pattern, string input)
{
    var regex = new Regex(pattern, RegexOptions.Singleline);
    var match = regex.Match(input);
    if (match.Groups.Count == 1)
    {
        return null;
    }
    return match.Groups[1].Value;
}

public static List<string> RegexOutMulti(string pattern, string input)
{
    var results = new List<string>();
    var regex = new Regex(pattern, RegexOptions.Singleline);
    var matches = regex.Matches(input);
    foreach (Match match in matches)
    {
        results.Add(match.Groups[1].Value);
    }
    return results;
}
}