
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

Top5Giftees(interests, feb2022);
Top3GiftersByMoney(interests, feb2022);
Top3GiftersByNumberOfMPs(interests, feb2022);

Recap(interests, latestRegisterUpdateStartDate, latestRegisterUpdateEndDate);

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

void Recap(IEnumerable<Interest> interests, DateTime start, DateTime end)
{
    var results = interests
        .Where(interest => interest.dateRegistered >= start && interest.dateRegistered <= end);

    var tweet = $@"In the latest update of the register of MP's financial interests ({start.ToString("dd MMM")} - {end.ToString("dd MMM")}) {results.Select(i => i.mp).Distinct().Count()} MPs have registered a total of {results.Count()} gifts with a combined value of {results.Sum(i => i.valueInPounds):C}";

    Console.WriteLine(tweet);
    Console.WriteLine(tweet.Length);
    Console.WriteLine();
}

void Top5Giftees(IEnumerable<Interest> interests, DateTime month)
{
    var results = interests
        .Where(interest => interest.dateRegistered?.Year == month.Year && interest.dateRegistered?.Month == month.Month)
        .GroupBy(interest => interest.mp, (mp, interests) => new
        {
            mp = mp,
            interests = interests
        })
        .OrderByDescending(x => x.interests.Sum(i => i.valueInPounds))
        .Take(5)
        .ToList();
    // .ForEach(x => {
    //     Console.WriteLine(x.mp);
    //     Console.WriteLine(x.interests.Sum(i => i.valueInPounds));
    //     Console.WriteLine();
    // });


    var tweet = $@"Most gifted MPs of the last month (gifts reg. in {month.ToString("MMM yyyy")})
    1. {results[0].mp.ToTweetFormat()} - {results[0].interests.Sum(i => i.valueInPounds):C}
    2. {results[1].mp.ToTweetFormat()} - {results[1].interests.Sum(i => i.valueInPounds):C}
    3. {results[2].mp.ToTweetFormat()} - {results[2].interests.Sum(i => i.valueInPounds):C}";

    Console.WriteLine(tweet);
    Console.WriteLine(tweet.Length);
    Console.WriteLine();
}

void Top3GiftersByMoney(IEnumerable<Interest> interests, DateTime month)
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
    1. {results[0].donor?.name} - {results[0].interests.Sum(i => i.valueInPounds):C}
    2. {results[1].donor?.name} - {results[1].interests.Sum(i => i.valueInPounds):C}
    3. {results[2].donor?.name} - {results[2].interests.Sum(i => i.valueInPounds):C}";

    Console.WriteLine(tweet);
    Console.WriteLine(tweet.Length);
    Console.WriteLine();
}

void Top3GiftersByNumberOfMPs(IEnumerable<Interest> interests, DateTime month)
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
