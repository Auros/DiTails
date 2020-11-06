using Zenject;
using SiraUtil.Tools;
using BeatSaverSharp;
using System.Threading;
using System.Threading.Tasks;
using Version = SemVer.Version;
using System.Collections.Generic;

namespace DiTails
{
    internal class LevelDataService : ILateDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly BeatSaver _beatSaverInstance;
        private readonly Dictionary<string, Beatmap> _mapCache = new Dictionary<string, Beatmap>();

        internal LevelDataService(SiraLog siraLog, [Inject(Id = "dev.auros.ditails.version")] Version version)
        {
            _siraLog = siraLog;
            _beatSaverInstance = new BeatSaver("DiTails", version.ToString());
        }

        public void LateDispose()
        {
            _beatSaverInstance.Dispose();
        }

        internal async Task<Beatmap?> GetBeatmap(IDifficultyBeatmap difficultyBeatmap, CancellationToken token)
        {
            if (!difficultyBeatmap.level.levelID.Contains("custom_level_"))
            {
                return null;
            }
            var hash = difficultyBeatmap.level.levelID.Replace("custom_level_", "");
            if (!_mapCache.TryGetValue(hash, out Beatmap? beatmap))
            {
                _siraLog.Debug($"Getting BeatSaver Level Data for {difficultyBeatmap.level.songName} ({hash})");
                beatmap = await _beatSaverInstance.Hash(hash, new StandardRequestOptions { Token = token });
                if (beatmap == null)
                {
                    _siraLog.Debug($"Could Not Find Level Data for {difficultyBeatmap.level.songName} ({hash})");
                    return null;
                }
                _siraLog.Debug($"Found Level Data for {difficultyBeatmap.level.songName} ({hash})");
                _mapCache.Add(hash, beatmap);
            }
            else
            {
                await beatmap.RefreshStats();
            }
            return beatmap ?? null;
        }
    }
}