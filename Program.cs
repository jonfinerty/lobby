using System.Text.RegularExpressions;

HttpClient client = new HttpClient();
client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.74 Safari/537.36");
client.DefaultRequestHeaders.Add("cookie", "uk-parliament.cookie-policy=eyJhbmFseXRpY3MiOnRydWUsIm1hcmtldGluZyI6dHJ1ZSwicHJlZmVyZW5jZXNfc2V0Ijp0cnVlfQ==; ai_user=096QT|2022-03-18T18:34:34.376Z; _hjSessionUser_464108=eyJpZCI6ImRjZmFmZDQxLTEyNTQtNTllNC1iMzUxLTAwMTY4NThhZWQ4NyIsImNyZWF0ZWQiOjE2NDc2Mjg0NzQ1NTUsImV4aXN0aW5nIjpmYWxzZX0=; __utmz=264344334.1647628489.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); _hjSessionUser_1134021=eyJpZCI6ImFkZDY1ODY2LTY1MDktNTNkMS1iYTU1LWIyODMyNDJlYjlkYyIsImNyZWF0ZWQiOjE2NDc2Mjg0ODA1MTMsImV4aXN0aW5nIjp0cnVlfQ==; _ga_YRC5R6CGNQ=GS1.1.1647628474.1.1.1647628827.60; __utma=264344334.598715478.1647628474.1647628489.1647715860.2; __utmc=264344334; __utmt=1; _gid=GA1.2.1390487863.1647715861; _gat_UA-15845045-54=1; __cf_bm=H9o7cjGiVbN5MFhTh4y1Fu4QJ4JFee46hXEKIzGa.Ig-1647715859-0-Ac4tTo8shdHHwoNFVXU3EkCsmgsyiJx/N/U5VRJAGfWpIP5CqF8bYHD2u8MkWC1Y9S61FHMMBjrOHJ3QmfoVfzXxSjiveVUFfGp0lfyXZXbTv367+hI9Frc5L2iIHj4ydBYq2Ed/XmLaTEa0hcvWT+mKnrDVndUGgIVWgLm9iggD; _hjIncludedInSessionSample=1; _hjSession_1134021=eyJpZCI6IjRmNjMzZmYxLWFkYmMtNGEzMC1iMjk4LWY3MzE0ZjZjMzhjNCIsImNyZWF0ZWQiOjE2NDc3MTU4NjA4MDMsImluU2FtcGxlIjp0cnVlfQ==; _hjIncludedInPageviewSample=1; _hjAbsoluteSessionInProgress=1; _ga_QQVTWCSLDS=GS1.1.1647715860.2.1.1647715869.0; _ga=GA1.2.598715478.1647628474; __utmb=264344334.2.10.1647715860; ai_session=4eu75|1647715860831|1647715869898");
var path = "https://publications.parliament.uk/pa/cm/cmregmem/220314/contents.htm";
var content = await GetPage(client, path);
var mpLinks = ParseMPLinks(content);
var interests = new List<Interest>();
foreach (var mpLink in mpLinks) {
    var mpInterests = await ParseMPPage(client, mpLink);
    interests.AddRange(mpInterests);
}

var groupedByDonor = interests.GroupBy(interest => interest.donor, (donor, interests) => new {
    Donor = donor,
    TotalValue = interests.Sum(interest => interest.valueInPounds),
    MPs = interests.Select(interest => interest.mp)
}).OrderBy(x => x.TotalValue);

foreach(var donorGroup in groupedByDonor) {
    Console.WriteLine(donorGroup.Donor);
    Console.WriteLine("£" + donorGroup.TotalValue);
    foreach(var mp in donorGroup.MPs) {
        Console.WriteLine(mp);
    }
    Console.WriteLine("----------------");
}


// var mpsWhoLoveAGamble = mpsAndInterests
//     .Where(mp => mp.Interests.Any(interest => interest.donor?.name == "Betting and Gaming Council"))
//     .Select(mp => mp.Item1).ToList();
// mpsWhoLoveAGamble.ForEach(Console.WriteLine);
// var giftMarchLeaderboard = mpsAndInterests
//     .Select(x => (x.MP, TotalValue: x.Interests
//                                      .Where(interest => interest.dateRegistered >= new DateTime(2022, 03, 01))
//                                      .Sum(interest => interest.valueInPounds)))
//     .OrderByDescending(x => x.TotalValue)
//     .Take(10).ToList();
// giftMarchLeaderboard.ForEach(x => Console.WriteLine($"{x.MP} - £{x.TotalValue}"));

// var totalGiftValue = mpsAndInterests.Sum(x => x.Interests.Sum(interest => interest.valueInPounds));
// Console.WriteLine(totalGiftValue);


async Task<List<Interest>> ParseMPPage(HttpClient client, string link)
{
    // todo, get date out of this
    var url = "https://publications.parliament.uk/pa/cm/cmregmem/220314/" + link;

    // let the parsing fun begin
    var content = await GetPage(client, url);
    var mp = MP.ParseMP(content);
    var interests = ParseUKGifts(mp, content);
    return interests;
}

List<Interest> ParseUKGifts(MP mp, string content)
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
        var gift = ParseUKGift(mp, rawGift);
        gifts.Add(gift);
    }
    return gifts;
}

GiftFromUKSource ParseUKGift(MP mp, string content)
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
    return new GiftFromUKSource(mp, donor, parsedValue, parsedDateReceived, parsedDateAccepted, parsedDateRegistered, parsedDateUpdated, description);
}

IDate? ParseDateOrRange(string? text)
{
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

    var seperators = new string[] { " - ", "-", " – ", "–", " to " };
    foreach (var seperator in seperators)
    {
        if (text.Contains(seperator))
        {
            return ParseDateRange(text, seperator);
        }
    }
    return ParseDate(text);
}

DateRange ParseDateRange(string text, string seperator)
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

public record GiftFromUKSource(MP mp, Donor? donor, decimal? valueInPounds, IDate? dateReceived, IDate? dateAccepted, Date? dateRegistered, Date? dateUpdated, string description) : Interest(mp, donor, valueInPounds, dateRegistered, dateUpdated);

public record Visit();

public record Interest(MP mp, Donor donor, decimal? valueInPounds, Date? dateRegistered, Date? dateUpdated);

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