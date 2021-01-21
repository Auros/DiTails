using System.Collections.Generic;
using Newtonsoft.Json;

namespace DiTails.Beatmap
{
    public class Metadata
    {
        [JsonProperty("songName")]
        public string? SongName { get; set; }

        [JsonProperty("songSubName")]
        public string? SongSubName { get; set; }

        [JsonProperty("songAuthorName")]
        public string? SongAuthorName { get; set; }

        [JsonProperty("levelAuthorName")]
        public string? LevelAuthorName { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("bpm")]
        public float BPM { get; set; }

        [JsonProperty("difficulties")]
        public Difficulties? Difficulties { get; set; }

        [JsonProperty("characteristics")]
        public List<BeatmapCharacteristic>? Characteristics { get; set; }
    }
}