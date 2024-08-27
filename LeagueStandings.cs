public class LeagueStandings
{
    public List<StandingResult> Results { get; set; }
}

public class StandingResult
{
    public int EventTotal { get; set; }
    public string PlayerName { get; set; }
    public int Rank { get; set; }
    public int Total { get; set; }
    public int Entry { get; set; }
}

public class TeamHistory
{
    public List<EventPoints> Current { get; set; }
}

public class EventPoints
{
    public int Event { get; set; }
    public int Points { get; set; }
}
