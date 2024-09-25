using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyLeaguePointsFetcher.Models
{
    internal class PlayerModel
    {
        public int entry { get; set; } //Entry = id
        public string player_name { get; set; } //Spelarnamn
        public string entry_name { get; set;} // Lagnamn
        public Dictionary<int, int> GameweekScores { get; set; } //Multiple gw's and points
    }
}
