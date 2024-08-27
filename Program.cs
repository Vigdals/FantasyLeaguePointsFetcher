using System.Diagnostics;
using System.Text.Json;
using FantasyLeaguePointsFetcher.Models;
using FantasyLeaguePointsFetcher.Resources;
using OfficeOpenXml.FormulaParsing.Logging;
using static Microsoft.IO.RecyclableMemoryStreamManager;

var LigaInfo = await GetLeagueInfo();

//for kvar deltakar i liga, hent ut GW score
foreach (var deltakar in LigaInfo)
{
    var PlayerInfo = await GetPlayerInfo(deltakar.entry);

    Debug.WriteLine(PlayerInfo);
}

Debug.WriteLine("");




async Task<List<Result>> GetLeagueInfo()
{
    //Lager ei tom liste av lag i ligaen
    var fplTeamsInLeagueList = new List<Result>();

    //Hardkoda standings url for Luster FPL
    string apiLeagueStandings = "https://fantasy.premierleague.com/api/leagues-classic/1008641/standings/";
    var jsonResult = await ApiCall.DoApiCallAsync(apiLeagueStandings);

    //Deserialiserer json og henter ut relevant data. Clunky but it funks
    JsonElement bigleagueJson = JsonSerializer.Deserialize<JsonElement>(jsonResult);
    JsonElement standingsJson = bigleagueJson.GetProperty("standings");
    JsonElement resultsStandingsJson = standingsJson.GetProperty("results");

    foreach (JsonElement jsonElement in resultsStandingsJson.EnumerateArray())
    {
        var player_name = jsonElement.GetProperty("player_name").GetString();
        var totalt_poeng = jsonElement.GetProperty("event_total").GetInt32();
        var lagID = jsonElement.GetProperty("entry").GetInt32();

        Debug.WriteLine(player_name + " : " + totalt_poeng + " og lagID er: " + lagID);

        var playerInLeague = new FantasyLeaguePointsFetcher.Models.Result
        {
            player_name = player_name,
            total = totalt_poeng,
            entry = lagID
        };

        fplTeamsInLeagueList.Add(playerInLeague);
    }

    return fplTeamsInLeagueList;
}

async Task<List<FantasyLeaguePointsFetcher.Models.Current>> GetPlayerInfo(int lagID)
{
    //Lager ei tom liste av lag i ligaen
    var fplPlayerList = new List<Current>();

    string apiPlayerHistory = "https://fantasy.premierleague.com/api/entry/" + lagID + "/history/";
    var jsonResult = await ApiCall.DoApiCallAsync(apiPlayerHistory);

    JsonElement PlayerHistory = JsonSerializer.Deserialize<JsonElement>(jsonResult);
    JsonElement PlayerHistory2 = PlayerHistory.GetProperty("current");

    foreach (JsonElement gwElement in PlayerHistory2.EnumerateArray())
    {
        var gw = gwElement.GetProperty("event").GetInt32();
        var points = gwElement.GetProperty("points").GetInt32();
        var points_on_bench = gwElement.GetProperty("points_on_bench").GetInt32();

        var playerGWs = new FantasyLeaguePointsFetcher.Models.Current
        {
            @event = gw,
            points = points,
            points_on_bench = points_on_bench
        };

        fplPlayerList.Add(playerGWs);
    }

   

    return fplPlayerList;
}