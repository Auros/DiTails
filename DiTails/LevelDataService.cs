using BeatSaverSharp;
using BeatSaverSharp.Models;
using IPA.Loader;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace DiTails
{
    internal class LevelDataService : ILateDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly BeatSaver _beatSaver;
        private readonly IPlatformUserModel _platformUserModel;

        internal LevelDataService(SiraLog siraLog, IPlatformUserModel platformUserModel, UBinder<Plugin, PluginMetadata> metadataBinder)
        {
            _siraLog = siraLog;
            _platformUserModel = platformUserModel;
            _beatSaver = new BeatSaver("DiTails", Version.Parse(metadataBinder.Value.HVersion.ToString()));
        }

        public void LateDispose()
        {
            _beatSaver.Clear();
            _beatSaver.Dispose();
        }

        internal async Task<Beatmap?> GetBeatmap(IDifficultyBeatmap difficultyBeatmap, CancellationToken token)
        {
            if (!difficultyBeatmap.level.levelID.Contains("custom_level_"))
            {
                return null;
            }
            var hash = difficultyBeatmap.level.levelID.Replace("custom_level_", "");
            var beatmap = await _beatSaver.BeatmapByHash(hash, token);
            return beatmap ?? null;
        }

        internal async Task<Beatmap> Vote(Beatmap beatmap, bool upvote, CancellationToken token)
        {
            try
            {
                bool steam = false;
                if (_platformUserModel is SteamPlatformUserModel)
                {
                    steam = true;
                }
                else if (!(_platformUserModel is OculusPlatformUserModel))
                {
                    _siraLog.Debug("Current platform cannot vote.");
                    return beatmap;
                }

                var info = await _platformUserModel.GetUserInfo(CancellationToken.None);
                var authToken = await _platformUserModel.GetUserAuthToken();
                var ticket = authToken.token;

                _siraLog.Debug("Starting Vote...");
                if (steam)
                {
                    ticket = ticket.Replace("-", "");
                }
                else
                {
                    ticket = authToken.token;
                }

                var response = await beatmap.LatestVersion.Vote(upvote ? BeatSaverSharp.Models.Vote.Type.Upvote : BeatSaverSharp.Models.Vote.Type.Downvote,
                    steam ? BeatSaverSharp.Models.Vote.Platform.Steam : BeatSaverSharp.Models.Vote.Platform.Oculus,
                    info.platformUserId,
                    ticket, token);

                _siraLog.Info(response.Successful);
                _siraLog.Info(response.Error ?? "good");
                if (response.Successful)
                {
                    await beatmap.Refresh();
                }
                _siraLog.Debug($"Voted. Upvote? ({upvote})");
            }
            catch (Exception e)
            {
                _siraLog.Error(e.Message);
            }
            return beatmap;
        }
    }
}