using System.Collections.Generic;
using Newtonsoft.Json;

namespace DiTails.Beatmap
{
    public struct BeatmapCharacteristic
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("difficulties")]
        public Dictionary<string, BeatmapCharacteristicDifficulty?> Difficulties { get; set; }
    }
}