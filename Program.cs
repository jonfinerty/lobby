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
    .Where(mp => mp.Interests.Any(interest => interest.donor.name == "Betting and Gaming Council"))
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


async Task<(MP MP, List<Interest> Interests)> ParseMPPage(HttpClient client, string link) {
    // todo, get date out of this
    var url = "https://publications.parliament.uk/pa/cm/cmregmem/220314/" + link;

    // let the parsing fun begin
    var content = await GetPage(client, url);
    var MP = ParseMP(content);
    var gifts = ParseUKGifts(content);
    return (MP, gifts);
}

MP ParseMP(string content) {
    var mpNameAndConstiuencyRegex = new Regex(@"class=""RegisterOfInterestsMemberHeader"">(.+)\s\((.+)\)</p>");
    var match = mpNameAndConstiuencyRegex.Match(content);
    var name = match.Groups[1].Value;
    var consituency = match.Groups[2].Value;
    return new MP(name, consituency);
}

List<Interest> ParseUKGifts(string content) {
    var gifts = new List<Interest>();
    var rawGifts = RegexOut(@"<strong>3\. Gifts, benefits and hospitality from UK sources</strong></p>(.+?)(<strong>|class=""spacer"")", content); // end with strong or class="prevNext"
    if (rawGifts == null) {
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
    var rawGiftList = RegexOutMulti(@"<p xmlns=""http://www.w3.org/1999/xhtml"" class=""indent"">(.+?)</p>", rawGifts);
    foreach(var rawGift in rawGiftList) {
        var gift = ParseUKGift(rawGift);
        gifts.Add(gift);
    }
    return gifts;
}

GiftFromUKSource ParseUKGift(string content) {
    var donorName = RegexOut(@"Name of donor: (.+?)<br/>", content);
    var donorAddress = RegexOut(@"Address of donor: (.+?)<br/>", content);
    var description = RegexOut(@"Amount of donation or nature and value if donation in kind: (.+?)<br/>", content);
    var value = RegexOut(@"£(.+?)(\s|<br/>|;|\)|\.)", content); // todo: multiple values
    var dateReceived = RegexOut(@"Date received: (.+?)( \(|<br/>)", content); // todo: fix range handling
    var dateAccepted = RegexOut(@"Date accepted: (.+?)( \(|<br/>)", content);
    var rawDonorStatus = RegexOut(@"Donor status: (.+?)<br/>", content);
    var dateRegistered = RegexOut(@"\(Registered (.+?)(\)|;)", content);
    var dateUpdated = RegexOut(@"\(Registered.+; updated (.+?)\)", content);

    decimal? parsedValue = null;
    try {
        parsedValue = string.IsNullOrEmpty(value) ? null : decimal.Parse(value);
    } catch(FormatException e) {
        Console.WriteLine(value);
        throw e;
    }
    var parsedDateReceived = ParseDateTime(dateReceived);
    var parsedDateAccepted = ParseDateTime(dateAccepted);
    var parsedDateRegistered = ParseDate(dateRegistered);
    var parsedDateUpdated = ParseDateTime(dateUpdated);

    // todo: properly regex out donor status
    var donorStatus = new CompanyDonorStatus(rawDonorStatus);
    var donor = new Donor(donorName, donorAddress, donorStatus);
    return new GiftFromUKSource(donor, parsedValue, parsedDateReceived, parsedDateAccepted, parsedDateRegistered, parsedDateUpdated, description);
}

Date? ParseDate(string text) {
    // if (text == "22 November 20201") {
    //     text = "22 November 2021";
    // }

    if (text == null) {
        return null;
    }

    var date = DateTime.Parse(text);
    if (date < new DateTime(2010,1,1) || date > new DateTime(2023,1,1)) {
        throw new Exception("Date fails sanity check: " + text);
    }

    return new Date(DateTime.Parse(text));
}

DateTime? ParseDateTime(string text) {
    if (text == null) {
        return null;
    }

    if (text == "22 November 20201") {
        text = "22 November 2021";
    }

    if (text.Contains("-")) {
        text = text.Split("-").Last();
    }
    if (text.Contains("–")) {
        text = text.Split("–").Last();
    }
    if (text.Contains(" to ")) {
        text = text.Split(" to ").Last();
    }
    return DateTime.Parse(text);
}

List<string> ParseMPLinks(string body) {
    var links = new List<string>();
    var mpLinkRegex = new Regex(@"<p xmlns=""http://www\.w3\.org/1999/xhtml"">\s+<a href=""(.+)"">.*</a>\s+</p>");
    var matches = mpLinkRegex.Matches(body);
    foreach (Match match in matches) {
        var link = match.Groups[1].Value;
        links.Add(link);
    }

    return links;
}

string RegexOut(string pattern, string input) {
    var regex = new Regex(pattern, RegexOptions.Singleline);
    var match = regex.Match(input);
    if (match.Groups.Count == 1) {
        return null;
    }
    return match.Groups[1].Value;
}

List<string> RegexOutMulti(string pattern, string input) {
    var results = new List<string>();
    var regex = new Regex(pattern, RegexOptions.Singleline);
    var matches = regex.Matches(input);
    foreach(Match match in matches)
    {
        results.Add(match.Groups[1].Value);
    }
    return results;
}

async Task<string> GetPage(HttpClient client, string path) {
    var uri = new Uri(path);
    var filename = uri.Segments.Last();
    var cachePath = "webcache/" + filename;
    if (File.Exists(cachePath)){
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
    } else {
        Console.WriteLine(response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine(body);
        throw new Exception("UH OH");
    }
}

public record MP(string name, string consituency);

public record GiftFromUKSource(Donor donor, decimal? valueInPounds, DateTime? dateReceived, DateTime? dateAccepted, Date? dateRegistered, DateTime? dateUpdated, string description) : Interest(donor, valueInPounds, dateRegistered, dateUpdated);

public record Visit();

public record Interest(Donor donor, decimal? valueInPounds, Date? dateRegistered, DateTime? dateUpdated);

public record Donor(string name, string address, DonorStatus status);

public record DonorStatus();

public record CompanyDonorStatus(string registrationNumber) : DonorStatus();

public record Date(DateTime dateTime) : IDate, IEquatable<DateTime>, IComparable<DateTime> {
    public bool Equals(DateTime other) {
        return dateTime.Equals(other);
    }

    public int CompareTo(DateTime other)
    {
        if (other == null) return 1;
        return dateTime.CompareTo(other);
    }

    public static bool operator >  (Date operand1, DateTime operand2)
    {
        if (operand1 == null) {
            return false;
        }
       return operand1.CompareTo(operand2) > 0;
    }

    public static bool operator <  (Date operand1, DateTime operand2)
    {
        if (operand1 == null) {
            return true;
        }
       return operand1.CompareTo(operand2) < 0;
    }

    public static bool operator >=  (Date operand1, DateTime operand2)
    {
        if (operand1 == null) {
            return false;
        }
       return operand1.CompareTo(operand2) >= 0;
    }

    public static bool operator <=  (Date operand1, DateTime operand2)
    {
        if (operand1 == null) {
            return true;
        }
       return operand1.CompareTo(operand2) <= 0;
    }

}

public record DateRange(DateTime startDate, DateTime endDate) : IDate;

public interface IDate {}