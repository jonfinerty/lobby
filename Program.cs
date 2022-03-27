
var interests = await Interest.GetInterests("220314");

var latestRegisterUpdateEndDate = new DateTime(2022, 03, 14);
var previousRegisterUpdateEndDate = new DateTime(2022, 02, 28);
var latestRegisterUpdateStartDate = previousRegisterUpdateEndDate.AddDays(1);
var feb2022 = new DateTime(2022, 02, 01);

//var interestSinceLastRegister = interests.Where(interest => interest.dateRegistered >= latestRegisterUpdateStartDate);
// var interestInTheLastMonth = interests.Where(interest => interest.dateRegistered >= l);
// interestSinceLastRegister.Select(InterestToTweets).ToList()
// .ForEach(i =>
// {
//     Console.WriteLine(i[0]);
//     Console.WriteLine(i[1]);
//     Console.WriteLine();
// });

Top3GifteesByMonth(interests, feb2022);
Top3GiftersByMoneyByMonth(interests, feb2022);
Top3GiftersByNumberOfMPsByMonth(interests, feb2022);

RegisterUpdateSummary(interests, latestRegisterUpdateStartDate, latestRegisterUpdateEndDate);
YearlySummary(interests);
YearlyTop3Giftees(interests);
YearlyTop3GiftersByMoney(interests);

// var groupedByDonor = interests.GroupBy(interest => interest.donor, (donor, interests) => new {
//     Donor = donor,
//     TotalValue = interests.Sum(interest => interest.valueInPounds),
//     MPs = interests.Select(interest => interest.mp)
// }).OrderBy(x => x.TotalValue);

// foreach(var donorGroup in groupedByDonor) {
//     Console.WriteLine(donorGroup.Donor);
//     Console.WriteLine("£" + donorGroup.TotalValue);
//     foreach(var mp in donorGroup.MPs) {
//         Console.WriteLine(mp);
//     }
//     Console.WriteLine("----------------");
// }

void RegisterUpdateSummary(IEnumerable<Interest> interests, DateTime start, DateTime end)
{
    var results = interests
        .Where(interest => interest.dateRegistered >= start && interest.dateRegistered <= end);

    var tweet = $@"In the latest update of the register of MP's financial interests ({start.ToString("dd MMM")} - {end.ToString("dd MMM")}) {results.Select(i => i.mp).Distinct().Count()} MPs have registered a total of {results.Count()} gifts with a combined value of {results.Sum(i => i.valueInPounds):C}";

    Console.WriteLine(tweet);
    Console.WriteLine(tweet.Length);
    Console.WriteLine();
}

void YearlySummary(IEnumerable<Interest> interests)
{
    var results = interests
        .Where(interest => interest.dateRegistered >= new DateTime(2022, 01, 01));

    var tweet = $@"Since the start of the year {results.Select(i => i.mp).Distinct().Count()} MPs have registered a total of {results.Count()} gifts from UK sources with a combined value of {results.Sum(i => i.valueInPounds):C}";
    Console.WriteLine(tweet);
    Console.WriteLine(tweet.Length);
    Console.WriteLine();
}

void YearlyTop3Giftees(IEnumerable<Interest> interests)
{
    var results = interests
        .Where(interest => interest.dateRegistered >= new DateTime(2022, 01, 01))
        .GroupBy(interest => interest.mp, (mp, interests) => new
        {
            mp = mp,
            interests = interests
        })
        .OrderByDescending(x => x.interests.Sum(i => i.valueInPounds))
        .Take(3)
        .ToList();

    var tweet = $@"Most gifted MPs so far this year

1. {results[0].mp.ToTweetFormat()} - {FormatGifts(results[0].interests.Count())} worth a total of {results[0].interests.Sum(i => i.valueInPounds):C}
2. {results[1].mp.ToTweetFormat()} - {FormatGifts(results[0].interests.Count())} worth a total of {results[1].interests.Sum(i => i.valueInPounds):C}
3. {results[2].mp.ToTweetFormat()} - {FormatGifts(results[0].interests.Count())} worth a total of {results[2].interests.Sum(i => i.valueInPounds):C}";

    Console.WriteLine(tweet);
    Console.WriteLine(tweet.Length);
    Console.WriteLine();
}

void YearlyTop3GiftersByMoney(IEnumerable<Interest> interests)
{
    var results = interests
        .Where(interest => interest.dateRegistered >= new DateTime(2022, 01, 01))
    .Where(interest => interest.donor != null)
    .GroupBy(interest => interest.donor, (donor, interests) => new
    {
        donor = donor,
        giftCount = interests.Count(),
        mps = interests.Select(interest => interest.mp).Distinct(),
        totalValue = interests.Sum(interest => interest.valueInPounds)
    })
    .OrderByDescending(x => x.totalValue)
    .Take(3)
    .ToList();

    var tweet = $@"Largest donors to MPs so far this year

1. {results[0].donor?.name} - {FormatGiftsToMPs(results[0].giftCount, results[0].mps.Count(), results[0].totalValue)}
2. {results[1].donor?.name} - {FormatGiftsToMPs(results[1].giftCount, results[1].mps.Count(), results[1].totalValue)}
3. {results[2].donor?.name} - {FormatGiftsToMPs(results[2].giftCount, results[2].mps.Count(), results[2].totalValue)}";

    Console.WriteLine(tweet);
    Console.WriteLine(tweet.Length);
    Console.WriteLine();
}

string FormatGifts(int giftCount)
{
    if (giftCount == 1)
    {
        return "1 gift";
    }
    else
    {
        return $"{giftCount} gifts";
    }
}

string FormatGiftsToMPs(int giftCount, int mpCount, decimal? totalValue)
{
    var result = FormatGifts(giftCount);

    if (mpCount == 1)
    {
        result += " to 1 MP";
    }
    else
    {
        result += $" to {mpCount} MPs";
    }

    result += $" worth a total of {totalValue:C}";

    return result;
}

void Top3GifteesByMonth(IEnumerable<Interest> interests, DateTime month)
{
    var results = interests
        .Where(interest => interest.dateRegistered?.Year == month.Year && interest.dateRegistered?.Month == month.Month)
        .GroupBy(interest => interest.mp, (mp, interests) => new
        {
            mp = mp,
            interests = interests
        })
        .OrderByDescending(x => x.interests.Sum(i => i.valueInPounds))
        .Take(3)
        .ToList();

    var tweet = $@"Most gifted MPs of the last month (gifts reg. in {month.ToString("MMM yyyy")})

1. {results[0].mp.ToTweetFormat()} - {FormatInterestsSum(results[0].interests)}
2. {results[1].mp.ToTweetFormat()} - {FormatInterestsSum(results[1].interests)}
3. {results[2].mp.ToTweetFormat()} - {FormatInterestsSum(results[2].interests)}";

    Console.WriteLine(tweet);
    Console.WriteLine(tweet.Length);
    Console.WriteLine();
}

void Top3GiftersByMoneyByMonth(IEnumerable<Interest> interests, DateTime month)
{
    var results = interests
    .Where(interest => interest.dateRegistered?.Year == month.Year && interest.dateRegistered?.Month == month.Month)
    .Where(interest => interest.donor != null)
    .GroupBy(interest => interest.donor, (donor, interests) => new
    {
        donor = donor,
        interests = interests
    })
    .OrderByDescending(x => x.interests.Sum(i => i.valueInPounds))
    .Take(3)
    .ToList();

    var tweet = $@"Largest donors to MPs in the last month (gifts reg. in {month.ToString("MMM yyyy")})

1. {results[0].donor?.name} - {FormatInterestsSum(results[0].interests)}
2. {results[1].donor?.name} - {FormatInterestsSum(results[1].interests)}
3. {results[2].donor?.name} - {FormatInterestsSum(results[2].interests)}";

    Console.WriteLine(tweet);
    Console.WriteLine(tweet.Length);
    Console.WriteLine();
}

string FormatInterestsSum(IEnumerable<Interest> interests)
{
    return $"{interests.Sum(i => i.valueInPounds):C}";
}

void Top3GiftersByNumberOfMPsByMonth(IEnumerable<Interest> interests, DateTime month)
{
    var results = interests
    .Where(interest => interest.dateRegistered?.Year == month.Year && interest.dateRegistered?.Month == month.Month)
    .Where(interest => interest.donor != null)
    .GroupBy(interest => interest.donor, (donor, interests) => new
    {
        donor = donor,
        mps = interests.Select(interest => interest.mp).Distinct(),
        totalValue = interests.Sum(interest => interest.valueInPounds)
    })
    .OrderByDescending(x => x.mps.Count())
    .Take(3)
    .ToList();

    var tweet = $@"Most widespread donors to MPs in the last month (gifts reg. in {month.ToString("MMM yyyy")})
    1. {results[0].donor?.name} - {results[0].totalValue:C} spread across {results[0].mps.Count()} MPs
    2. {results[1].donor?.name} - {results[1].totalValue:C} spread across {results[1].mps.Count()} MPs
    3. {results[2].donor?.name} - {results[2].totalValue:C} spread across {results[2].mps.Count()} MPs";

    Console.WriteLine(tweet);
    Console.WriteLine(tweet.Length);
    Console.WriteLine();
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
