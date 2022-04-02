using System.Text.RegularExpressions;

public record GiftFromUKSource(MP mp, Donor? donor, decimal? valueInPounds, DateRange? dateReceived, DateTime? dateAccepted, DateTime? dateRegistered, DateTime? dateUpdated, string description) : Interest(mp, donor, valueInPounds, dateAccepted, dateRegistered, dateUpdated, description);

public record Visit();

public record Interest(MP mp, Donor? donor, decimal? valueInPounds, DateTime? dateAccepted, DateTime? dateRegistered, DateTime? dateUpdated, string description)
{

    // datestamp example: 220314
    public static async Task<List<Interest>> GetInterests(string datestamp)
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.82 Safari/537.36");
        client.DefaultRequestHeaders.Add("cookie", "uk-parliament.cookie-policy=eyJhbmFseXRpY3MiOnRydWUsIm1hcmtldGluZyI6dHJ1ZSwicHJlZmVyZW5jZXNfc2V0Ijp0cnVlfQ==; ai_user=Af+r|2022-03-18T21:45:16.952Z; _hjSessionUser_464108=eyJpZCI6ImExOTZmMDhjLTY4NTktNTg3Yi04ZmFhLTE1M2FjN2RiMzE2MSIsImNyZWF0ZWQiOjE2NDc2Mzk5MTcwOTQsImV4aXN0aW5nIjp0cnVlfQ==; _hjSessionUser_1134021=eyJpZCI6ImMwOWQ0OWI1LWM0YjctNTMzYS05ZDJmLTYzZmVlZjVkYzkwYyIsImNyZWF0ZWQiOjE2NDc2Mzk5MzIzOTksImV4aXN0aW5nIjp0cnVlfQ==; __utmz=264344334.1647643322.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); cf_clearance=F_xeoX6u8sVdMLLEhg8DMRJzVOuJTDXtV27X09RODI8-1647770114-0-250; _hjSessionUser_1480997=eyJpZCI6IjU5MTNlNWFhLWY2OWMtNWM1ZC1iOTdjLTY4M2IxOWE5NTVhZCIsImNyZWF0ZWQiOjE2NDc3NzE3ODU5NDYsImV4aXN0aW5nIjpmYWxzZX0=; _ga_CL6BBQMGES=GS1.1.1647771785.1.0.1647771793.52; __utma=264344334.1194589672.1647639917.1647799941.1647802884.8; _gid=GA1.2.1397537186.1648294845; _ga_YRC5R6CGNQ=GS1.1.1648294844.5.0.1648294851.53; _ga_QQVTWCSLDS=GS1.1.1648294851.10.1.1648294912.60; _ga=GA1.1.1194589672.1647639917");

        var path = "https://publications.parliament.uk/pa/cm/cmregmem/" + datestamp + "/contents.htm";
        var content = await GetPage(client, path, datestamp);
        var mpLinks = ParseMPLinks(content);
        var interests = new List<Interest>();
        foreach (var mpLink in mpLinks)
        {
            var mpInterests = await ParseMPPage(client, mpLink, datestamp);
            interests.AddRange(mpInterests);
        }

        return interests;
    }


    static async Task<List<Interest>> ParseMPPage(HttpClient client, string link, string datestamp)
    {
        // todo, get date out of this
        var url = "https://publications.parliament.uk/pa/cm/cmregmem/" + datestamp + "/" + link;

        // let the parsing fun begin
        var content = await GetPage(client, url, datestamp);
        var mp = MP.ParseMP(url, content);
        var interests = ParseUKGifts(mp, content);
        return interests;
    }

    static List<Interest> ParseUKGifts(MP mp, string content)
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

    static GiftFromUKSource ParseUKGift(MP mp, string content)
    {

        // Amount of donation, or nature and value if donation in kind
        var description = ParseDescription(content);
        var values = Utils.RegexOutMulti(@"£([0-9,]+\.?[0-9]*)(\s|<br/>|;|\)|\.)", content);
        var value = values.Count == 0 ? null : values.Last(); // todo: multiple values
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

        var parsedDateReceived = Utils.ParseDateRange(dateReceived);
        var parsedDateAccepted = Utils.ParseDateRange(dateAccepted)?.startDate;
        var parsedDateRegistered = Utils.ParseDate(dateRegistered);
        var parsedDateUpdated = Utils.ParseDate(dateUpdated);

        var donor = Donor.ParseDonor(content);
        return new GiftFromUKSource(mp, donor, parsedValue, parsedDateReceived, parsedDateAccepted, parsedDateRegistered, parsedDateUpdated, description);
    }

    static string ParseDescription(string content)
    {
        //>Amount of donation or nature and value if donation in kind:
        var description = Utils.RegexOut(@"Amount of donation or nature and value if donation in kind: (.+?)(, value|, total value|; value|<br/>)", content);
        if (description == null)
        {
            description = Utils.RegexOut(@"Amount of donation, or nature and value if benefit in kind: (.+?)(, value|, total value|; value|<br/>)", content);
        }

        if (description == null)
        {
            return content;
        }

        return description;
    }

    static List<string> ParseMPLinks(string body)
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

    static async Task<string> GetPage(HttpClient client, string path, string datestamp)
    {
        var uri = new Uri(path);
        var filename = uri.Segments.Last();
        var cacheDir = "webcache/" + datestamp;
        var cachePath = cacheDir + "/" + filename;
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
            Directory.CreateDirectory(cacheDir);
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



}
