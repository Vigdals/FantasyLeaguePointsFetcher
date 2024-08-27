using System.Diagnostics;
using System.Text.Json;
using FantasyLeaguePointsFetcher.Models;
using FantasyLeaguePointsFetcher.Resources;

var LigaInfo = await GetTeamInfo();

async Task<List<Standings>> GetTeamInfo()
{
    //Lager ei tom liste av lag i ligaen
    var fplTeamsInLeagueList = new List<Standings>();

    //Hardkoda standings url for Luster FPL
    string apiLeagueStandings = "https://fantasy.premierleague.com/api/leagues-classic/1008641/standings/";
    var jsonResult = await ApiCall.DoApiCallAsync(apiLeagueStandings);

    //Deserialiserer json og henter ut relevant data
    JsonElement bigleagueJson = JsonSerializer.Deserialize<JsonElement>(jsonResult);
    JsonElement standingsJson = bigleagueJson.GetProperty("standings");
    JsonElement resultsStandingsJson = standingsJson.GetProperty("results");

    foreach (JsonElement jsonElement in resultsStandingsJson.EnumerateArray())
    {
        var player_name = jsonElement.GetProperty("player_name").GetString();
        var totalt_poeng = jsonElement.GetProperty("event_total").GetInt32();
        var lagID = jsonElement.GetProperty("entry").GetInt32();

        Debug.WriteLine(player_name + " : " + totalt_poeng + " og lagID er: " + lagID);
    }

    return fplTeamsInLeagueList;
}