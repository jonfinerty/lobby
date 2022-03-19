using System.Text.RegularExpressions;

HttpClient client = new HttpClient();
client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");
client.DefaultRequestHeaders.Add("cookie", "__utmc=264344334; __utmz=264344334.1647644283.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); _gid=GA1.2.1528732911.1647644284; ai_user=UQyUd|2022-03-18T22:58:03.874Z; _hjSessionUser_1134021=eyJpZCI6IjFjOTEyNWIxLWI2ODItNWJmNi05OGNiLWVhNWVjNzNiMDk5ZSIsImNyZWF0ZWQiOjE2NDc2NDQyODQxMzksImV4aXN0aW5nIjp0cnVlfQ==; __utma=264344334.13601410.1647644283.1647644283.1647681763.2; __utmt=1; __utmb=264344334.1.10.1647681763; _ga_QQVTWCSLDS=GS1.1.1647681761.2.1.1647681762.0; _ga=GA1.2.13601410.1647644283; _gat_UA-15845045-54=1; __cf_bm=1UBr0M55Lgw3SIoPV6nT4HH6KbUzptjs.VQjfQuKsiI-1647681763-0-AfANkTx8rc7ybsnp0s6B3cpxpjT4inr0jR/qB+XAF3mMc0PBfB3QHqk/s2bXqmfBvCaGPiIWDPSoGTilOpaC2HqU1N2BT1Et6yYT6aqnbt4+9HwJOluym48lcLByvN3Nr6M06xhoM/4wMfkz/RCAg0n753iHJeYa7g1Oz1U/UTlw; ai_session=2bSwW|1647681763293.1|1647681763293.1; _hjIncludedInSessionSample=1; _hjSession_1134021=eyJpZCI6IjE4MWRhNDYwLTQwN2ItNDAzMi1iM2QzLTkxMjMwNDMxMTYyMSIsImNyZWF0ZWQiOjE2NDc2ODE3NjM4ODcsImluU2FtcGxlIjp0cnVlfQ==; _hjIncludedInPageviewSample=1; _hjAbsoluteSessionInProgress=1");
var path = "https://publications.parliament.uk/pa/cm/cmregmem/220314/contents.htm";
var content = await GetPage(client, path);
var mpLinks = ParseMPLinks(content);
var mpsAndInterests = await Task.WhenAll(mpLinks.Select(async link => await ParseMPPage(client, link)));
// foreach(var mpAndInterests in mpsAndInterests) {
//     Console.WriteLine(mpAndInterests.Item1);
//     foreach(var interest in mpAndInterests.Item2) {
//         Console.WriteLine("\t" + interest);
//     }
// }
var mpsWhoLoveAGamble = mpsAndInterests
    .Where(mp => mp.Interests.Any(interest => interest.donor?.name == "Betting and Gaming Council"))
    .Select(mp => mp.Item1).ToList();
mpsWhoLoveAGamble.ForEach(Console.WriteLine);
var giftMarchLeaderboard = mpsAndInterests
    .Select(x => (x.MP, TotalValue: x.Interests
                                     .Where(interest => interest.dateRegistered >= new DateTime(2022, 03, 01))
                                     .Sum(interest => interest.valueInPounds)))
    .OrderByDescending(x => x.TotalValue)
    .Take(10).ToList();
giftMarchLeaderboard.ForEach(x => Console.WriteLine($"{x.MP} - £{x.TotalValue}"));

var totalGiftValue = mpsAndInterests.Sum(x => x.Interests.Sum(interest => interest.valueInPounds));
Console.WriteLine(totalGiftValue);


async Task<(MP MP, List<Interest> Interests)> ParseMPPage(HttpClient client, string link)
{
    // todo, get date out of this
    var url = "https://publications.parliament.uk/pa/cm/cmregmem/220314/" + link;

    // let the parsing fun begin
    var content = await GetPage(client, url);
    var MP = ParseMP(content);
    var gifts = ParseUKGifts(content);
    return (MP, gifts);
}

MP ParseMP(string content)
{
    var mpNameAndConstiuencyRegex = new Regex(@"class=""RegisterOfInterestsMemberHeader"">(.+)\s\((.+)\)</p>");
    var match = mpNameAndConstiuencyRegex.Match(content);
    var name = match.Groups[1].Value;
    var consituency = match.Groups[2].Value;
    return new MP(name, consituency);
}

List<Interest> ParseUKGifts(string content)
{
    var gifts = new List<Interest>();
    var rawGifts = Utils.RegexOut(@"<strong>3\. Gifts, benefits and hospitality from UK sources</strong></p>(.+?)(<strong>|class=""spacer"")", content); // end with strong or class="prevNext"
    if (rawGifts == null)
    {
        return gifts;
    }

    /*
<p xmlns="http://www.w3.org/1999/xhtml" class="indent">Name of donor: Power Leisure Bookmakers Ltd<br/>
Address of donor: Waterfront, Chancellors Road, London W6 9HP<br/>
Amount of donation or nature and value if donation in kind: Ticket with hospitality to attend the Euro 2020 tournament, value £1,961<br/>
Date received: 29 June 2021<br/>
Date accepted: 29 June 2021<br/>
Donor status: company, registration 03822566<br/>
(Registered 02 August 2021)</p>
    */
    var rawGiftList = Utils.RegexOutMulti(@"<p xmlns=""http://www.w3.org/1999/xhtml"" class=""indent"">(.+?)</p>", rawGifts);
    foreach (var rawGift in rawGiftList)
    {
        var gift = ParseUKGift(rawGift);
        gifts.Add(gift);
    }
    return gifts;
}

GiftFromUKSource ParseUKGift(string content)
{
 
    var description = Utils.RegexOut(@"Amount of donation or nature and value if donation in kind: (.+?)<br/>", content);
    var value = Utils.RegexOut(@"£(.+?)(\s|<br/>|;|\)|\.)", content); // todo: multiple values
    var dateReceived = Utils.RegexOut(@"Date received: (.+?)( \(|<br/>)", content); // todo: fix range handling
    var dateAccepted = Utils.RegexOut(@"Date accepted: (.+?)( \(|<br/>)", content);
    var dateRegistered = Utils.RegexOut(@"\(Registered (.+?)(\)|;)", content);
    var dateUpdated = Utils.RegexOut(@"\(Registered.+; updated (.+?)\)", content);

    decimal? parsedValue = null;
    try
    {
        parsedValue = string.IsNullOrEmpty(value) ? null : decimal.Parse(value);
    }
    catch (FormatException e)
    {
        Console.WriteLine(value);
        throw e;
    }

    var parsedDateReceived = ParseDateOrRange(dateReceived);
    var parsedDateAccepted = ParseDateOrRange(dateAccepted);
    var parsedDateRegistered = ParseDate(dateRegistered);
    var parsedDateUpdated = ParseDate(dateUpdated);

    var donor = Donor.ParseDonor(content);
    return new GiftFromUKSource(donor, parsedValue, parsedDateReceived, parsedDateAccepted, parsedDateRegistered, parsedDateUpdated, description);
}

IDate? ParseDateOrRange(string? text) {
    if (text == null)
    {
        return null;
    }

    if (text == "22 November 20201")
    {
        text = "22 November 2021";
    }

    if (text.Contains("-"))
    {
        return ParseDateRange(text, "-");
    }

    var seperators = new string[]{" - ", "-", " – ", "–", " to "};
    foreach (var seperator in seperators) {
        if (text.Contains(seperator)) {
            return ParseDateRange(text, seperator);
        }
    }
    return ParseDate(text);
}

DateRange ParseDateRange(string text, string seperator) {
    var endDate = DateTime.Parse(text.Split(seperator).Last());
    
    var startDateRaw = text.Split(seperator).First();
    DateTime startDate;

    var intParseSuccess = int.TryParse(startDateRaw, out var startDay);
    if (intParseSuccess) {
        startDate = new DateTime(endDate.Year, endDate.Month, startDay);
    } else {
        startDate = DateTime.Parse(startDateRaw);
    }

    return new DateRange(startDate, endDate);
}

Date? ParseDate(string? text)
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

    return new Date(DateTime.Parse(text));
}

List<string> ParseMPLinks(string body)
{
    var links = new List<string>();
    var mpLinkRegex = new Regex(@"<p xmlns=""http://www\.w3\.org/1999/xhtml"">\s+<a href=""(.+)"">.*</a>\s+</p>");
    var matches = mpLinkRegex.Matches(body);
    foreach (Match match in matches)
    {
        var link = match.Groups[1].Value;
        links.Add(link);
    }

    return links;
}

async Task<string> GetPage(HttpClient client, string path)
{
    var uri = new Uri(path);
    var filename = uri.Segments.Last();
    var cachePath = "webcache/" + filename;
    if (File.Exists(cachePath))
    {
        //Console.WriteLine("Loading from cache: " + cachePath);
        return File.ReadAllText(cachePath);
    }

    //Console.WriteLine("Making API Call: " + path);
    HttpResponseMessage response = await client.GetAsync(uri);
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
        File.WriteAllText(cachePath, content);
        return content;
    }
    else
    {
        Console.WriteLine(response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine(body);
        throw new Exception("UH OH");
    }
}

public record MP(string name, string consituency);

public record GiftFromUKSource(Donor? donor, decimal? valueInPounds, IDate? dateReceived, IDate? dateAccepted, Date? dateRegistered, Date? dateUpdated, string description) : Interest(donor, valueInPounds, dateRegistered, dateUpdated);

public record Visit();

public record Interest(Donor donor, decimal? valueInPounds, Date? dateRegistered, Date? dateUpdated);

public record Date(DateTime dateTime) : IDate, IEquatable<DateTime>, IComparable<DateTime>
{
    public bool Equals(DateTime other)
    {
        return dateTime.Equals(other);
    }

    public int CompareTo(DateTime other)
    {
        return dateTime.CompareTo(other);
    }

    public static bool operator >(Date operand1, DateTime operand2)
    {
        if (operand1 == null)
        {
            return false;
        }
        return operand1.CompareTo(operand2) > 0;
    }

    public static bool operator <(Date operand1, DateTime operand2)
    {
        if (operand1 == null)
        {
            return true;
        }
        return operand1.CompareTo(operand2) < 0;
    }

    public static bool operator >=(Date operand1, DateTime operand2)
    {
        if (operand1 == null)
        {
            return false;
        }
        return operand1.CompareTo(operand2) >= 0;
    }

    public static bool operator <=(Date operand1, DateTime operand2)
    {
        if (operand1 == null)
        {
            return true;
        }
        return operand1.CompareTo(operand2) <= 0;
    }

}

public record DateRange(DateTime startDate, DateTime endDate) : IDate;

public interface IDate { }