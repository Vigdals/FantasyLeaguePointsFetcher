// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

namespace FantasyLeaguePointsFetcher.Models
{
    public class League
{
    public int id { get; set; }
    public string name { get; set; }
    public DateTime created { get; set; }
    public bool closed { get; set; }
    public object max_entries { get; set; }
    public string league_type { get; set; }
    public string scoring { get; set; }
    public int admin_entry { get; set; }
    public int start_event { get; set; }
    public string code_privacy { get; set; }
    public bool has_cup { get; set; }
    public object cup_league { get; set; }
    public object rank { get; set; }
}

public class NewEntries
{
    public bool has_next { get; set; }
    public int page { get; set; }
    public List<object> results { get; set; }
}

public class Result
{
    public int id { get; set; }
    public int event_total { get; set; }
    public string player_name { get; set; }
    public int rank { get; set; }
    public int last_rank { get; set; }
    public int rank_sort { get; set; }
    public int total { get; set; }
    public int entry { get; set; }
    public string entry_name { get; set; }
}

public class Root2
{
    public NewEntries new_entries { get; set; }
    public DateTime last_updated_data { get; set; }
    public League league { get; set; }
    public Result standings { get; set; }
}

public class Results
{
    public bool has_next { get; set; }
    public int page { get; set; }
    public List<Result> results { get; set; }
}
}


