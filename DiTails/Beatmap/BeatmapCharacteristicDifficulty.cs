using Newtonsoft.Json;

namespace DiTails.Beatmap
{
    public struct BeatmapCharacteristicDifficulty
    {
        [JsonProperty("duration")]
        public float Duration { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("bombs")]
        public int Bombs { get; set; }

        [JsonProperty("notes")]
        public int Notes { get; set; }

        [JsonProperty("obstacles")]
        public int Obstacles { get; set; }

        [JsonProperty("njs")]
        public float NoteJumpSpeed { get; set; }

        [JsonProperty("njsOffset")]
        public float NoteJumpSpeedOffset { get; set; }
    }
}