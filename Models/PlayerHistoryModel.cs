using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyLeaguePointsFetcher.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Current
    {
        public int @event { get; set; }
        public int points { get; set; }
        public int total_points { get; set; }
        public int rank { get; set; }
        public int rank_sort { get; set; }
        public int overall_rank { get; set; }
        public int percentile_rank { get; set; }
        public int bank { get; set; }
        public int value { get; set; }
        public int event_transfers { get; set; }
        public int event_transfers_cost { get; set; }
        public int points_on_bench { get; set; }
        public string playerName { get; set; }
    }

    public class Past
    {
        public string season_name { get; set; }
        public int total_points { get; set; }
        public int rank { get; set; }
    }

    public class Root
    {
        public List<Current> current { get; set; }
        public List<Past> past { get; set; }
        public List<object> chips { get; set; }
    }


}
