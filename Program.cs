
HttpClient client = new HttpClient();
client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");
client.DefaultRequestHeaders.Add("cookie", "__utma=264344334.13601410.1647644283.1647644283.1647644283.1; __utmc=264344334; __utmz=264344334.1647644283.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); __utmt=1; __utmb=264344334.1.10.1647644283; _gid=GA1.2.1528732911.1647644284; _gat_UA-15845045-54=1; ai_user=UQyUd|2022-03-18T22:58:03.874Z; __cf_bm=WZgCRZYyMJ9UPfE7AYkvY3ufcM7vStd2mM1sYfh02p0-1647644256-0-AZL9jJNf5uW5CmlpvP8pnhI5bXZKCUbLqAD/6tiOkRErGfN86i38XQfkHHO0vHmxGaod4d3E9+Zj5UD+VOlpMNmFR58T7WUl35NIUDDjVsQ5I6qLoofy1W/k2pXEYo/BkOTSv7/ZACn99LX/Sf0oG5ppWmHhs8O3MZ+SiLAUTQgz; _ga_QQVTWCSLDS=GS1.1.1647644283.1.0.1647644283.0; _ga=GA1.1.13601410.1647644283; ai_session=V3VY0|1647644284017|1647644284017; _hjSessionUser_1134021=eyJpZCI6IjFjOTEyNWIxLWI2ODItNWJmNi05OGNiLWVhNWVjNzNiMDk5ZSIsImNyZWF0ZWQiOjE2NDc2NDQyODQxMzksImV4aXN0aW5nIjpmYWxzZX0=; _hjFirstSeen=1; _hjIncludedInSessionSample=1; _hjSession_1134021=eyJpZCI6ImY5MGQxMGMyLTA0NTMtNDQ3Ni1iMGYzLTk4ZmQyZjA0NjhmMSIsImNyZWF0ZWQiOjE2NDc2NDQyODQyODEsImluU2FtcGxlIjp0cnVlfQ==; _hjIncludedInPageviewSample=1; _hjAbsoluteSessionInProgress=0");
var path = "https://publications.parliament.uk/pa/cm/cmregmem/220314/contents.htm";
HttpResponseMessage response = await client.GetAsync(path);
Console.WriteLine(response.StatusCode);

    var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    Console.WriteLine(body);
if (response.IsSuccessStatusCode)
{
    //var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    //Console.WriteLine(body);
} else {
    throw new Exception("AH");
}

public record MP(string name, string consituency, string twitterHandle);

public record Gift(Donor donor, Decimal amountInPounds, DateTime dateReceived, DateTime dateAccepted, DateTime dateRegistered, string description);

public record Visit();

public record Donor(string name, string address, DonorStatus status);

public record DonorStatus();

public record CompanyDonorStatus(string registrationNumber) : DonorStatus();