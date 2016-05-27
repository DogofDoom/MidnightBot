using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Modules.Osu
{
    public class Map
    {
        public int approved { get; set; }
        public string approved_date { get; set; }
        public string last_update { get; set; }
        public string artist { get; set; }
        public int beatmap_id { get; set; }
        public long beatmapset_id { get; set; }
        public int bpm { get; set; }
        public string creator { get; set; }
        public float difficultyrating { get; set; }
        public int diff_size { get; set; }
        public int diff_overall { get; set; }
        public int diff_approach { get; set; }
        public int diff_drain { get; set; }
        public int hit_length { get; set; }
        public string source { get; set; }
        public int genre_id { get; set; }
        public int language_id { get; set; }
        public string title { get; set; }
        public int total_length { get; set; }
        public string version { get; set; }
        public string file_md5 { get; set; }
        public int mode { get; set; }
        public string tags { get; set; }
        public int favourite_count { get; set; }
        public int playcount { get; set; }
        public int passcount { get; set; }
        public int max_combo { get; set; }
    }
}
