using System.Diagnostics;
using System.Text.Json;
using FantasyLeaguePointsFetcher.Models;
using FantasyLeaguePointsFetcher.Resources;

var LigaInfo = await GetLeagueInfo();

//for kvar deltakar i liga, hent ut GW score
foreach (var deltakar in LigaInfo)
{
    Debug.WriteLine(deltakar.player_name + ":");
    //Henter ut GW data for gitt player
    var PlayerInfo = await GetPlayerInfo(deltakar.entry);

    foreach (var info in PlayerInfo) Debug.WriteLine("GW" + info.@event + " " + info.points);
}





async Task<List<Result>> GetLeagueInfo()
{
    //Lager ei tom liste av lag i ligaen
    var fplTeamsInLeagueList = new List<Result>();

    //Hardkoda standings url for Luster FPL
    var apiLeagueStandings = "https://fantasy.premierleague.com/api/leagues-classic/1008641/standings/";
    var jsonResult = await ApiCall.DoApiCallAsync(apiLeagueStandings);

    //Deserialiserer json og henter ut relevant data. Clunky but it funks
    var bigleagueJson = JsonSerializer.Deserialize<JsonElement>(jsonResult);
    var standingsJson = bigleagueJson.GetProperty("standings");
    var resultsStandingsJson = standingsJson.GetProperty("results");

    foreach (var jsonElement in resultsStandingsJson.EnumerateArray())
    {
        var player_name = jsonElement.GetProperty("player_name").GetString();
        var totalt_poeng = jsonElement.GetProperty("event_total").GetInt32();
        var lagID = jsonElement.GetProperty("entry").GetInt32();

        var playerInLeague = new Result
        {
            player_name = player_name,
            total = totalt_poeng,
            entry = lagID
        };

        fplTeamsInLeagueList.Add(playerInLeague);
    }

    return fplTeamsInLeagueList;
}

async Task<List<Current>> GetPlayerInfo(int lagID)
{
    //Lager ei tom liste av lag i ligaen
    var fplPlayerList = new List<Current>();

    var apiPlayerHistory = "https://fantasy.premierleague.com/api/entry/" + lagID + "/history/";
    var jsonResult = await ApiCall.DoApiCallAsync(apiPlayerHistory);

    var PlayerHistory = JsonSerializer.Deserialize<JsonElement>(jsonResult);
    var PlayerHistory2 = PlayerHistory.GetProperty("current");

    foreach (var gwElement in PlayerHistory2.EnumerateArray())
    {
        var gw = gwElement.GetProperty("event").GetInt32();
        var points = gwElement.GetProperty("points").GetInt32();
        var points_on_bench = gwElement.GetProperty("points_on_bench").GetInt32();

        var playerGWs = new Current
        {
            @event = gw,
            points = points,
            points_on_bench = points_on_bench
        };

        fplPlayerList.Add(playerGWs);
    }

    return fplPlayerList;
}