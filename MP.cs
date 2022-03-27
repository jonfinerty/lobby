using System.Text.RegularExpressions;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
public record MP(string name, string consituency, string? twitterHandle, string registerLink)
{

    public string ToTweetFormat()
    {
        if (twitterHandle == null)
        {
            return $"{name} ({consituency})";

        }

        return $"{twitterHandle} {name} ({consituency})";
    }

    public static MP ParseMP(string link, string content)
    {
        var mpNameAndConstiuencyRegex = new Regex(@"class=""RegisterOfInterestsMemberHeader"">([^\(]+)\s\((.+)\)</p>");
        var match = mpNameAndConstiuencyRegex.Match(content);
        //var name = match.Groups[1].Value;
        var consituency = match.Groups[2].Value;
        var twitterHandle = GetTwitterHandle(consituency);
        var name = GetFormattedName(consituency);
        return new MP(name, consituency, twitterHandle, link);
    }

    private static string GetFormattedName(string consituency)
    {
        using (var reader = new StreamReader("MPsonTwitter_list_name.csv"))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<TwitterMP>();
            try
            {
                return records.First(x => String.Equals(x.Constituency, consituency, StringComparison.InvariantCultureIgnoreCase)).Name;
            }
            catch (System.InvalidOperationException)
            {
                throw new Exception("Twitter information missing for consituency: " + consituency);
            }
        }
    }

    private static string? GetTwitterHandle(string consituency)
    {
        using (var reader = new StreamReader("MPsonTwitter_list_name.csv"))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<TwitterMP>();
            try
            {
                var result = records.First(x => String.Equals(x.Constituency, consituency, StringComparison.InvariantCultureIgnoreCase)).ScreenName;
                if (result == "")
                {
                    return null;
                }
                else
                {
                    return result;
                }
            }
            catch (System.InvalidOperationException)
            {
                throw new Exception("Twitter information missing for consituency: " + consituency);
            }
        }
    }


    public class TwitterMP
    {
        public string? Name { get; set; }
        [Name("Screen name")]
        public string? ScreenName { get; set; }
        public string? Constituency { get; set; }
    }

}
