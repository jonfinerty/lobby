
var interests = await Interest.GetInterests("220314");

var interestSinceLastRegister = interests.Where(interest => interest.dateRegistered > new DateTime(2022, 03, 01));
var interestInTheLastMonth = interests.Where(interest => interest.dateRegistered > new DateTime(2022, 02, 15));
interestSinceLastRegister.Select(InterestToTweets).ToList()
.ForEach(i =>
{
    Console.WriteLine(i[0]);
    Console.WriteLine(i[1]);
    Console.WriteLine();
});

Top5Giftees(interestSinceLastRegister);
Top3GiftersByMoney(interestInTheLastMonth);
Top3GiftersByNumberOfMPs(interestInTheLastMonth);

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
void Top5Giftees(IEnumerable<Interest> interests) {
    interests.GroupBy(interest => interest.mp, (mp, interests) => new {
        mp = mp,
        interests = interests 
    })
    .OrderByDescending(x => x.interests.Sum(i => i.valueInPounds))
    .Take(5)
    .ToList()
    .ForEach(x => {
        Console.WriteLine(x.mp);
        Console.WriteLine(x.interests.Sum(i => i.valueInPounds));
        Console.WriteLine();
    });
}

void Top3GiftersByMoney(IEnumerable<Interest> interests) {
    var results = interests.GroupBy(interest => interest.donor, (donor, interests) => new {
        donor = donor,
        interests = interests 
    })
    .OrderByDescending(x => x.interests.Sum(i => i.valueInPounds))
    .Take(3)
    .ToList();

    var tweet = $@"Top donors of the last month (X-Xth) to MPs
    {results[0].donor.name} - {results[0].interests.Sum(i => i.valueInPounds):C}
    {results[1].donor.name} - {results[1].interests.Sum(i => i.valueInPounds):C}
    {results[2].donor.name} - {results[2].interests.Sum(i => i.valueInPounds):C}";

    Console.WriteLine(tweet);
    Console.WriteLine(tweet.Length);
}

void Top3GiftersByNumberOfMPs(IEnumerable<Interest> interests) {
    var results = interests.GroupBy(interest => interest.donor, (donor, interests) => new {
        donor = donor,
        mps = interests.Select(interest => interest.mp).Distinct()
    })
    .OrderByDescending(x => x.mps.Count())
    .Take(3)
    .ToList();
    // .ForEach(x => {
    //     Console.WriteLine(x.donor);
    //     foreach (var mp in x.mps) {
    //         Console.WriteLine(mp);
    //     }
    //     Console.WriteLine();
    // });

    var tweet = $"The  of the last month are";
}


string[] InterestToTweets(Interest interest)
{
    return new string[]{
        $"{interest.mp.twitterHandle} MP for {interest.mp.consituency} accepted a gift worth {interest.valueInPounds:C} from {interest.donor?.name} of \"{interest.description}\"",
        $"The gift was accepted on {interest.dateAccepted?.ToString("dd MMMM yyyy")} and registered on {interest.dateRegistered?.ToString("dd MMMM yyyy")}. All official details here: {interest.mp.registerLink}"
    };
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
