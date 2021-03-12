using System;
using Zenject;
using IPA.Loader;
using IPA.Utilities;
using SiraUtil.Tools;
using System.Net.Http;
using Newtonsoft.Json;
using SiraUtil.Zenject;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DiTails
{
    internal class LevelDataService : ILateDisposable
    {
        private readonly Http _http;
        private readonly SiraLog _siraLog;
        private readonly IPlatformUserModel _platformUserModel;
        private readonly string _beatSaverURL = "https://beatsaver.com/api";
        private readonly Dictionary<string, Beatmap.Beatmap> _mapCache = new Dictionary<string, Beatmap.Beatmap>();

        internal LevelDataService(Http http, SiraLog siraLog, IPlatformUserModel platformUserModel, UBinder<Plugin, PluginMetadata> metadataBinder)
        {
            _http = http;
            _siraLog = siraLog;
            _platformUserModel = platformUserModel;
        }

        public void LateDispose()
        {

        }

        internal async Task<Beatmap.Beatmap?> GetBeatmap(IDifficultyBeatmap difficultyBeatmap, CancellationToken token)
        {
            if (!difficultyBeatmap.level.levelID.Contains("custom_level_"))
            {
                return null;
            }
            var hash = difficultyBeatmap.level.levelID.Replace("custom_level_", "");
            if (!_mapCache.TryGetValue(hash, out Beatmap.Beatmap? beatmap))
            {
                _siraLog.Debug($"Getting BeatSaver Level Data for {difficultyBeatmap.level.songName} ({hash})");
                var response = await _http.GetAsync($"{_beatSaverURL}/maps/by-hash/{hash}", token: token);
                
                if (!response.Successful)
                {
                    _siraLog.Debug($"Could Not Find Level Data for {difficultyBeatmap.level.songName} ({hash})");
                    return null;
                }
                beatmap = JsonConvert.DeserializeObject<Beatmap.Beatmap>(response.Content!);
                _siraLog.Debug($"Found Level Data for {difficultyBeatmap.level.songName} ({hash})");
                _mapCache.Add(hash, beatmap);
            }
            return beatmap ?? null;
        }

        internal async Task<Beatmap.Beatmap> Vote(Beatmap.Beatmap beatmap, bool upvote, CancellationToken token)
        {
            try
            {
                var info = await _platformUserModel.GetUserInfo();
                var ticket = await _platformUserModel.GetUserAuthToken();

                _siraLog.Debug("Starting Vote...");
                var ticketBytes = Utils.StringToByteArray(ticket.token.Replace("-", ""));

                var response = await _http.PostAsync(
                    $"https://beatsaver.com/api/vote/steam/{beatmap.Key}",
                    JsonConvert.SerializeObject(new VotePayload(upvote ? VoteDirection.Up : VoteDirection.Down, info.platformUserId, ticketBytes)), token: token);

                if (response.Successful)
                {
                    beatmap = JsonConvert.DeserializeObject<Beatmap.Beatmap>(response.Content!);
                }
                _siraLog.Debug($"Voted. Upvote? ({upvote})");
            }
            catch (Exception e)
            {
                _siraLog.Error(e.Message);
            }
            return beatmap;
        }

        private struct VotePayload
        {
            [JsonProperty("steamID")]
            public string SteamID { readonly get; set; }

            [JsonProperty("ticket")]
            public string Ticket { readonly get; set; }

            [JsonProperty("direction")]
            public string Direction { readonly get; set; }

            public VotePayload(VoteDirection direction, string steamID, byte[] authTicket)
            {
                SteamID = steamID;
                Ticket = string.Concat(Array.ConvertAll(authTicket, (byte x) => x.ToString("X2")));
                short num = (short)direction;
                Direction = num.ToString();
            }
        }

        internal enum VoteDirection : short
        {
            Up = 1,
            Down = -1
        }
    }
}