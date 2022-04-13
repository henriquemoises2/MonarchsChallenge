using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

// For demonstration purposes only. In a real-life scenario this would be defined in a config file
string MonarchsGetUrl = "https://gist.githubusercontent.com/christianpanton/10d65ccef9f29de3acd49d97ed423736/raw/b09563bc0c4b318132c7a738e679d4f984ef0048/kings";

List<Monarch>? monarchsList = new List<Monarch>();

using (HttpClient httpClient = new HttpClient())
{
    HttpResponseMessage message = await httpClient.GetAsync(MonarchsGetUrl);
    string monarchsAsJson = await message.Content.ReadAsStringAsync();

    monarchsList = JsonSerializer.Deserialize<List<Monarch>>(monarchsAsJson);
}

// In a real-life scenario we would probably make many more field validations. 
if(monarchsList == null || !monarchsList.Any())
{
    Console.WriteLine("Invalid Data");
    return;
}

int listSize = monarchsList.Count;
string longestRullingMonarch = LongestRullingMonarch(monarchsList);
string longestRullingHouse = LongestRullingHouse(monarchsList);
string mostCommonFirstName = MostCommonFirstName(monarchsList);


Console.WriteLine("How many monarchs are there in the list?");
Console.WriteLine(listSize);
Console.WriteLine();

Console.WriteLine("Which monarch ruled the longest (and for how long)?");
Console.WriteLine(longestRullingMonarch);
Console.WriteLine();


Console.WriteLine("Which house ruled the longest (and for how long)?");
Console.WriteLine(longestRullingHouse);
Console.WriteLine();

Console.WriteLine("What was the most common first name?");
Console.WriteLine(mostCommonFirstName);
Console.WriteLine();

#region Helper Methods
string LongestRullingMonarch(IEnumerable<Monarch> monarchsList)
{
    // Associates each monarch to its rulling years and then sorts them by rulling years descending, selecting the first element
    Dictionary<string, int> rulerYearsPairs = monarchsList.ToDictionary(mon => mon?.Name ?? "", mon => mon.GetRullingYearsInterval);
    KeyValuePair<string, int> longestRullingMonarch = rulerYearsPairs.OrderByDescending(rul => rul.Value).FirstOrDefault();

    return $"{longestRullingMonarch.Key} ({longestRullingMonarch.Value} years)";
}

string LongestRullingHouse(IEnumerable<Monarch> monarchsList)
{
    // Groups each house, sums its rulling years and then sorts them by rulling years descending, selecting the first element
    IEnumerable<IGrouping<string, Monarch>> houseYearsGrouping = monarchsList.GroupBy(mon => mon?.House ?? "");
    Dictionary<string, int> houseYearsPairs = houseYearsGrouping.ToDictionary(hse => hse.Key, hse => hse.Sum(mon => mon.GetRullingYearsInterval));
    KeyValuePair<string, int> longestRullingHouse = houseYearsPairs.OrderByDescending(rul => rul.Value).FirstOrDefault();

    return $"{longestRullingHouse.Key} ({longestRullingHouse.Value} years)";
}

string MostCommonFirstName(IEnumerable<Monarch> monarchsList)
{
    List<string> firstNames = new List<string>();
    foreach(string? name in monarchsList.Select(mon => mon.Name))
    {
        if(name == null)
        {
            continue;
        }
        // Full name has more than one name
        if(name.Contains(" "))
        {
            string? firstName = name.Split(" ").FirstOrDefault();
            if(firstName != null)
            {
                firstNames.Add(firstName);
            }
        }
        else
        {
            firstNames.Add(name);
        }
    }
    // Groups all first names together, orders them by number of occurences descending and returns the first element
    IEnumerable<IGrouping<string, string>> namesGrouping = firstNames.GroupBy(mon => mon);
    IGrouping<string, string>? returningName = namesGrouping.OrderByDescending(grp => grp.Count()).FirstOrDefault();
    return returningName?.Key ?? "";

}

#endregion

// For demonstration purposes only. In a real-life scenario this class would be in a different file
public class Monarch
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nm")]
    public string? Name { get; set; }

    [JsonPropertyName("cty")]
    public string? City { get; set; }
    [JsonPropertyName("hse")]
    public string? House { get; set; }

    [JsonPropertyName("yrs")]
    public string? RullingYears { get; set; }


    public int GetRullingYearsInterval
    {
        get
        {
            if (string.IsNullOrEmpty(RullingYears))
            {
                return 0;
            }
            string[] years = RullingYears.Split('-');

            // Only have the starting year
            if (years.Length == 1)
            {
                return 0;
            }

            // In a real-life situation we could handle this with an exception
            if (!int.TryParse(years.ElementAt(0), out int startingYear))
            {
                return 0;
            }

            // If there is no ending year then the monarch is still rulling
            if(!int.TryParse(years.ElementAt(1), out int endingYear))
            {
                endingYear = DateTime.Now.Year;
            }

            return endingYear - startingYear;
        }
    }
}