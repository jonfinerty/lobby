
var interests = await Interest.GetInterests("220314");

var interestSinceLastRegister = interests.Where(interest => interest.dateRegistered > new DateTime(2022, 03, 01));
interestSinceLastRegister.Select(InterestToTweets).ToList()
.ForEach(i =>
{
    Console.WriteLine(i[0]);
    Console.WriteLine(i[1]);
    Console.WriteLine();
});
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

string[] InterestToTweets(Interest interest)
{
    return new string[]{
        $"{interest.mp.twitterHandle} MP for {interest.mp.consituency} accepted a gift worth {interest.valueInPounds:C} from {interest.donor.name} of \"{interest.description}\"",
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
