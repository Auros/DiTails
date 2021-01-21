using Newtonsoft.Json;

namespace DiTails.Beatmap
{
    public class Difficulties
    {
        [JsonProperty("easy")]
        public bool Easy { get; set; }

        [JsonProperty("normal")]
        public bool Normal { get; set; }

        [JsonProperty("hard")]
        public bool Hard { get; set; }

        [JsonProperty("expert")]
        public bool Expert { get; set; }

        [JsonProperty("expertPlus")]
        public bool ExpertPlus { get; set; }
    }
}