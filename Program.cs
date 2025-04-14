using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FantasyLeaguePointsFetcher.Models;
using FantasyLeaguePointsFetcher.Resources;
using OfficeOpenXml;

public class FantasyLeaguePointsFetcherApp
{
    private static readonly string ApiLeagueStandingsUrl = "https://fantasy.premierleague.com/api/leagues-classic/1008641/standings/";
    private static readonly string ExcelFilePathWindows = "C:\\Project\\fpl\\FantasyLeaguePointsFetcher\\FPL Luster totaloversikt.xlsx";
    private static readonly string ExcelFilePathMac = "/Users/adrianvigdal/Documents/fpl/FPL Luster totaloversikt.xlsx";
    private static readonly string ExcelFilePath = Environment.OSVersion.Platform == PlatformID.Unix ? ExcelFilePathMac : ExcelFilePathWindows;

    public async Task Run()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Fetch league information from the Fantasy Premier League API
        var leagueInfo = await GetLeagueInfo();

        // Sort the league information by player name alphabetically
        leagueInfo = leagueInfo.OrderBy(p => p.player_name).ToList();

        // Process the league data and write it to the Excel file
        await ProcessLeagueData(leagueInfo);
    }

    // Method to get information about all players in the league
    private async Task<List<Result>> GetLeagueInfo()
    {
        // List to store the information of teams/players in the league
        var fplTeamsInLeagueList = new List<Result>();

        // Make an API call to fetch the league standings as a JSON string
        var jsonResult = await ApiCall.DoApiCallAsync(ApiLeagueStandingsUrl);

        // Deserialize the JSON response to extract player information
        var bigleagueJson = JsonSerializer.Deserialize<JsonElement>(jsonResult);
        var standingsJson = bigleagueJson.GetProperty("standings");
        var resultsStandingsJson = standingsJson.GetProperty("results");

        // Loop through each player entry and extract relevant information
        foreach (var jsonElement in resultsStandingsJson.EnumerateArray())
        {
            var playerInLeague = new Result
            {
                player_name = jsonElement.GetProperty("player_name").GetString(),
                entry_name = jsonElement.GetProperty("entry_name").GetString(),
                total = jsonElement.GetProperty("event_total").GetInt32(),
                entry = jsonElement.GetProperty("entry").GetInt32()
            };

            // Add the player information to the list
            fplTeamsInLeagueList.Add(playerInLeague);
        }

        // Return the list of players in the league
        return fplTeamsInLeagueList;
    }

    // Method to get detailed gameweek information for a specific player
    private async Task<List<Current>> GetSpecificPlayerGWInfo(int lagID, string playerName)
    {
        // List to store gameweek data for the player
        var fplPlayerList = new List<Current>();

        // Construct the URL to fetch the player's gameweek history
        var apiPlayerHistory = $"https://fantasy.premierleague.com/api/entry/{lagID}/history/";

        // Make an API call to get the player's gameweek data
        var jsonResult = await ApiCall.DoApiCallAsync(apiPlayerHistory);

        // Deserialize the JSON response to extract gameweek information
        var playerHistory = JsonSerializer.Deserialize<JsonElement>(jsonResult);
        var playerHistoryDetails = playerHistory.GetProperty("current");

        // Loop through each gameweek entry and extract relevant details
        foreach (var gwElement in playerHistoryDetails.EnumerateArray())
        {
            var playerGWs = new Current
            {
                @event = gwElement.GetProperty("event").GetInt32(),
                points = gwElement.GetProperty("points").GetInt32(),
                points_on_bench = gwElement.GetProperty("points_on_bench").GetInt32(),
                playerName = playerName
            };

            // Add the gameweek information to the list
            fplPlayerList.Add(playerGWs);
        }

        // Return the list of gameweek information for the player
        return fplPlayerList;
    }

    // Method to process the fetched league data and write it to an Excel file
    private async Task ProcessLeagueData(List<Result> leagueInfo)
    {
        try
        {
            // Check if the Excel file exists
            if (!File.Exists(ExcelFilePath))
            {
                Console.WriteLine("Excel file not found.");
                return;
            }

            // Open the Excel file for editing
            using (var package = new ExcelPackage(new FileInfo(ExcelFilePath)))
            {
                // Get the worksheet where data will be written (Utrekning)
                var worksheetUtrekning = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == "Utrekning");
                var row = 4; // Start writing data from row 4

                // Loop through each player in the league and fetch their gameweek info
                foreach (var participant in leagueInfo)
                {
                    var playerName = participant.player_name;
                    var entryName = participant.entry_name;

                    // Fetch gameweek data for the current player
                    var playerGWInfo = await GetSpecificPlayerGWInfo(participant.entry, playerName);

                    // Write the player's entry name and player name to the Excel file
                    worksheetUtrekning.Cells[row, 1].Value = entryName;
                    worksheetUtrekning.Cells[row, 2].Value = playerName;

                    // Loop through each gameweek entry and write the points to the Excel file
                    foreach (var gwInfo in playerGWInfo)
                    {
                        worksheetUtrekning.Cells[row, gwInfo.@event + 2].Value = gwInfo.points;
                    }

                    row++; // Move to the next row for the next player
                }

                // Save the changes made to the Excel file
                package.Save();
                Console.WriteLine($"Data has been appended to {ExcelFilePath}");
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions that may occur during the process
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}

// Entry point for the program
public class Program
{
    public static async Task Main()
    {
        // Create an instance of the app and run it
        var app = new FantasyLeaguePointsFetcherApp();
        await app.Run();
    }
}
