using System.Text.RegularExpressions;

public class Utils
{
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

    public static DateRange? ParseDateRange(string? text)
    {
        if (text == null)
        {
            return null;
        }

        if (text == "22 November 20201")
        {
            text = "22 November 2021";
        }

        var seperators = new string[] { " - ", "-", " – ", "–", " to " };
        foreach (var seperator in seperators)
        {
            if (text.Contains(seperator))
            {
                var endDate = DateTime.Parse(text.Split(seperator).Last());

                var startDateRaw = text.Split(seperator).First();
                DateTime startDate;

                var intParseSuccess = int.TryParse(startDateRaw, out var startDay);
                if (intParseSuccess)
                {
                    startDate = new DateTime(endDate.Year, endDate.Month, startDay);
                }
                else
                {
                    startDate = DateTime.Parse(startDateRaw);
                }

                return new DateRange(startDate, endDate);
            }
        }
        var singleDayRange = ParseDate(text);
        if (singleDayRange == null)
        {
            return null;
        }
        else
        {
            return new DateRange(singleDayRange.Value, singleDayRange.Value);
        }
    }

    public static DateTime? ParseDate(string? text)
    {
        if (text == null)
        {
            return null;
        }

        var date = DateTime.Parse(text);
        if (date < new DateTime(2010, 1, 1) || date > new DateTime(2023, 1, 1))
        {
            throw new Exception("Date fails sanity check: " + text);
        }

        return DateTime.Parse(text);
    }

}

public record DateRange(DateTime startDate, DateTime endDate);