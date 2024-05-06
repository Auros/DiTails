using BeatSaverSharp;
using BeatSaverSharp.Models;
using IPA.Loader;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using System;
using System.Threading;
using System.Threading.Tasks;
using DiTails.Utilities;
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

        internal async Task<Beatmap?> GetBeatmap(BeatmapLevel level, CancellationToken token)
        {
            if (level.TryGetHash(out var hash))
            {
                var beatmap = await _beatSaver.BeatmapByHash(hash, token);
                return beatmap ?? null;
            }

            return null;
        }

        internal async Task<Beatmap> Vote(Beatmap beatmap, bool upvote, CancellationToken token)
        {
            try
            {
                bool steam = false;
                var info = await _platformUserModel.GetUserInfo(token);

                if (info.platform == UserInfo.Platform.Steam)
                {
                    steam = true;
                }
                else if (info.platform != UserInfo.Platform.Oculus)
                {
                    _siraLog.Debug("Current platform cannot vote.");
                    return beatmap;
                }

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