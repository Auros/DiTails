using Newtonsoft.Json;

namespace DiTails.Beatmap
{
    public class BSVUser
    {
        [JsonProperty("_id")]
        public string ID { get; set; } = null!;

        [JsonProperty("username")]
        public string Username { get; set; } = null!;
    }
}