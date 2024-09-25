using System.Text.Json;
using FantasyLeaguePointsFetcher.Models;
using FantasyLeaguePointsFetcher.Resources;
using OfficeOpenXml;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var LigaInfo = await GetLeagueInfo();

// sort fplPlayerList aplhabetically
LigaInfo = LigaInfo.OrderBy(p => p.player_name).ToList();

// for kvar deltakar i liga, hent ut GW score
//foreach (var deltakar in LigaInfo)
//{
//    Debug.WriteLine(deltakar.player_name + ":");
//    // Henter ut GW data for gitt player
//    var PlayerInfo = await GetPlayerInfo(deltakar.entry);

//    foreach (var info in PlayerInfo) Debug.WriteLine("GW" + info.@event + " " + info.points);
//}

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
        var entry_name = jsonElement.GetProperty("entry_name").GetString();
        var totalt_poeng = jsonElement.GetProperty("event_total").GetInt32();
        var lagID = jsonElement.GetProperty("entry").GetInt32();


        var playerInLeague = new Result
        {
            player_name = player_name,
            entry_name = entry_name,
            total = totalt_poeng,
            entry = lagID
        };

        fplTeamsInLeagueList.Add(playerInLeague);
    }

    return fplTeamsInLeagueList;
}

async Task<List<Current>> GetSpecificPlayerGWInfo(int lagID, string playerName)
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
            points_on_bench = points_on_bench,
            playerName = playerName
        };

        fplPlayerList.Add(playerGWs);
    }

    return fplPlayerList;
}

try
{
    // Open the existing workbook
    var filePath = "C:\\GitHub\\FantasyLeaguePointsFetcher\\FPL Luster totaloversikt.xlsx";
    using (var package = new ExcelPackage(new FileInfo(filePath)))
    {
        var playersInformation = new List<PlayerModel>();

        var worksheetUtrekning = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == "Utrekning");
        var worksheetGWOversikt = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == "GW oversikt");
        var row = 4; // Start at row 4 in Utrekning table. Cell is defined by GameWeek


        // Write data to Utrekning sheet
        foreach (var deltakar in LigaInfo)
        {
            // Write player info for each entry
            var playerName = deltakar.player_name;
            var entryName = deltakar.entry_name;
            var playerGWInfo = await GetSpecificPlayerGWInfo(deltakar.entry, playerName);

            var playerModel = new PlayerModel
            {
                player_name = playerName,
                entry_name = entryName,
                GameweekScores = new Dictionary<int, int>()
            };

            //adding player team name and normal name here:
            worksheetUtrekning.Cells[row, 1].Value = entryName;
            worksheetUtrekning.Cells[row, 2].Value = playerName;

            foreach (var gwInfo in playerGWInfo)
            {
                //sets the gameweek as an int for incrementing the gameweeks
                var gw = gwInfo.@event;
                worksheetUtrekning.Cells[row, gw + 2].Value = gwInfo.points;
                playerModel.GameweekScores[gw] = gwInfo.points;
            }

            playersInformation.Add(playerModel);
            row++;
        }

        package.Save();
    }

    Console.WriteLine("Data has been appended to " + filePath);
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

Console.WriteLine("Press the enter key to leave this window");
Console.ReadLine();