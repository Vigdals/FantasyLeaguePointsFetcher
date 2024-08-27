using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyLeaguePointsFetcher.Models
{
    internal class PlayerModel
    {
        public int entry { get; set; }
        public string player_name { get; set; }
        public string entry_name { get; set;}
       //public List<GWScores> GWScores { get; set; }

        public class GWScores
        {

        }

    }
}
