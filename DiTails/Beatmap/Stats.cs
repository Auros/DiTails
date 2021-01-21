using Newtonsoft.Json;

namespace DiTails.Beatmap
{
    public struct Stats
    {
        [JsonProperty("downloads")]
        public int Downloads { get; set; }

        [JsonProperty("plays")]
        public int Plays { get; set; }

        [JsonProperty("upVotes")]
        public int UpVotes { get; set; }

        [JsonProperty("downVotes")]
        public int DownVotes { get; set; }

        [JsonProperty("rating")]
        public float Rating { get; set; }

        [JsonProperty("heat")]
        public float Heat { get; set; }
    }
}