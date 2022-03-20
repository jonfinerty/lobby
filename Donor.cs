public record CompanyDonor(string name, string address, string registrationNumber) : Donor(name, address);
public record TradeUnionDonor(string name, string address) : Donor(name, address);
public record IndividualDonor(string name, string address) : Donor(name, address);
public record UnincorporatedAssociationDonor(string name, string address) : Donor(name, address);
public record IndustrialAndProvidentSocietyDonor(string name, string address, string registrationNumber) : Donor(name, address);
public record OtherDonor(string name, string address, string? type) : Donor(name, address);
public record FriendlySocietyDonor(string name, string address) : Donor(name, address);
public record LimitedLiabilityPartnershipDonor(string name, string address) : Donor(name, address);
public record Donor(string name, string address)
{
    public static Donor? ParseDonor(string content)
    {
        var donorName = Utils.RegexOut(@"Name of donor: (.+?)<br/>", content);
        var donorAddress = Utils.RegexOut(@"Address of donor: (.+?)<br/>", content);
        var rawDonorStatus = Utils.RegexOut(@"Donor status: (.+?)<br/>", content);

        if (donorName == null || donorAddress == null || rawDonorStatus == null)
        {
            return null;
        }

        Donor donor;
        if (rawDonorStatus.Contains("company"))
        {
            var registrationNumber = Utils.RegexOut(@"\s(\S+)$", rawDonorStatus);
            if (registrationNumber == null)
            {
                throw new Exception("Registration number null: " + content);
            }
            donor = new CompanyDonor(donorName, donorAddress, registrationNumber);
        }
        else if (rawDonorStatus.Contains("individual"))
        {
            donor = new IndividualDonor(donorName, donorAddress);
        }
        else if (rawDonorStatus.Contains("trade union"))
        {
            donor = new TradeUnionDonor(donorName, donorAddress);
        }
        else if (rawDonorStatus.Contains("unincorporated association"))
        {
            donor = new UnincorporatedAssociationDonor(donorName, donorAddress);
        }
        else if (rawDonorStatus.Contains("Industrial and Provident Society"))
        {
            var registrationNumber = Utils.RegexOut(@"\s(.+)$", rawDonorStatus);
            if (registrationNumber == null)
            {
                throw new Exception("Registration number null: " + content);
            }
            donor = new IndustrialAndProvidentSocietyDonor(donorName, donorAddress, registrationNumber);
        }
        else if (rawDonorStatus.Contains("other"))
        {
            var type = Utils.RegexOut(@"\s\((.+)\)$", rawDonorStatus);
            donor = new OtherDonor(donorName, donorAddress, type);
        }
        else if (rawDonorStatus.Contains("friendly society"))
        {
            donor = new FriendlySocietyDonor(donorName, donorAddress);
        }
        else if (rawDonorStatus.Contains("limited liability partnership"))
        {
            donor = new LimitedLiabilityPartnershipDonor(donorName, donorAddress);
        }
        else
        {
            Console.WriteLine(content);
            Console.WriteLine(rawDonorStatus);
            throw new Exception("Unknown donor type");
        }

        return donor;
    }
}