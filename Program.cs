
var interests = await Interest.GetInterests("220314");

var interestSinceLastRegister = interests.Where(interest => interest.dateRegistered > new DateTime(2022, 03, 01));
var interestInTheLastMonth = interests.Where(interest => interest.dateRegistered > new DateTime(2022, 02, 15));
// interestSinceLastRegister.Select(InterestToTweets).ToList()
// .ForEach(i =>
// {
//     Console.WriteLine(i[0]);
//     Console.WriteLine(i[1]);
//     Console.WriteLine();
// });

Top5Giftees(interests, new DateTime(2022, 02, 01));
Top3GiftersByMoney(interestInTheLastMonth, new DateTime(2022, 02, 01));
Top3GiftersByNumberOfMPs(interestInTheLastMonth, new DateTime(2022, 02, 01));

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

// Top 5 MPs by money received
// Top 3 gifters by money given
// Top 3 by number of MPs given to
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


    var tweet = $@"Most gifted MPs of the last month ({month.ToString("MMM yyyy")})
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
    .GroupBy(interest => interest.donor, (donor, interests) => new
    {
        donor = donor,
        interests = interests
    })
    .OrderByDescending(x => x.interests.Sum(i => i.valueInPounds))
    .Take(3)
    .ToList();

    var tweet = $@"Largest donors of the last month ({month.ToString("MMM yyyy")}) to MPs
    1. {results[0].donor.name} - {results[0].interests.Sum(i => i.valueInPounds):C}
    2. {results[1].donor.name} - {results[1].interests.Sum(i => i.valueInPounds):C}
    3. {results[2].donor.name} - {results[2].interests.Sum(i => i.valueInPounds):C}";

    Console.WriteLine(tweet);
    Console.WriteLine(tweet.Length);
    Console.WriteLine();
}

void Top3GiftersByNumberOfMPs(IEnumerable<Interest> interests, DateTime month)
{
    var results = interests
    .Where(interest => interest.dateRegistered?.Year == month.Year && interest.dateRegistered?.Month == month.Month)
    .GroupBy(interest => interest.donor, (donor, interests) => new
    {
        donor = donor,
        mps = interests.Select(interest => interest.mp).Distinct(),
        totalValue = interests.Sum(interest => interest.valueInPounds)
    })
    .OrderByDescending(x => x.mps.Count())
    .Take(3)
    .ToList();

    var tweet = $@"Most widespread donors of the last month ({month.ToString("MMM yyyy")}) are
    1. {results[0].donor.name} - {results[0].totalValue:C} spread across {results[0].mps.Count()} MPs
    2. {results[1].donor.name} - {results[1].totalValue:C} spread across {results[1].mps.Count()} MPs
    3. {results[2].donor.name} - {results[2].totalValue:C} spread across {results[2].mps.Count()} MPs";

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
