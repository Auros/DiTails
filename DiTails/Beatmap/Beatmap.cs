using System;
using Newtonsoft.Json;

namespace DiTails.Beatmap
{
    public class Beatmap
    {
        [JsonProperty("_id")]
        public string ID { get; set; } = null!;

        [JsonProperty("key")]
        public string Key { get; set; } = null!;

        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        [JsonProperty("description")]
        public string Description { get; set; } = null!;

        [JsonProperty("uploader")]
        public BSVUser Uploader { get; set; } = null!;

        [JsonProperty("uploaded")]
        public DateTime Uploaded { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; } = null!;

        [JsonProperty("stats")]
        public Stats Stats { get; set; }

        [JsonProperty("directDownload")]
        public string DirectDownload { get; set; } = null!;

        [JsonProperty("downloadURL")]
        public string DownloadURL { get; set; } = null!;

        [JsonProperty("coverURL")]
        public string CoverURL { get; set; } = null!;

        [JsonProperty("hash")]
        public string Hash { get; set; } = null!;
    }
}