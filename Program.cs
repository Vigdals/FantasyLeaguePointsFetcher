using System.Diagnostics;
using System.Text.Json;
using FantasyLeaguePointsFetcher.Models;
using FantasyLeaguePointsFetcher.Resources;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

try
{
    // Open the existing workbook
    var filePath = "C:\\GitHub\\FantasyLeaguePointsFetcher\\FPL Luster totaloversikt.xlsx";
    using (var package = new ExcelPackage(new FileInfo(filePath)))
    {
        // --- UTREKNING SHEET --- 
        var worksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == "Utrekning");

        if (worksheet == null)
            // If the worksheet doesn't exist, create a new one
            worksheet = package.Workbook.Worksheets.Add("Utrekning");

        var row = 4; // Start at row 4
        var cells = 3; // Starts at cell 3

        // Write data to Utrekning sheet
        foreach (var deltakar in LigaInfo)
        {
            // Write player info for each entry
            var PlayerInfo = await GetPlayerInfo(deltakar.entry);
            foreach (var info in PlayerInfo)
            {
                //sets the gameweek as an int for incrementing the gameweeks
                var gw = info.@event;
                worksheet.Cells[row, gw + 2].Value = info.points;
            }

            row++;
        }

        // --- GW Oversikt sheet ---


        // Save changes to the existing file
        package.Save();
    }

    Console.WriteLine("Data has been appended to FPL Luster totaloversikt.xlsx");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}