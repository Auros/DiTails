using SemVer;
using Zenject;
using BeatSaverSharp;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DiTails
{
    internal class LevelDataService : ILateDisposable
    {
        private readonly BeatSaver _beatSaverInstance;
        private readonly Dictionary<string, Beatmap> _mapCache = new Dictionary<string, Beatmap>();

        internal LevelDataService([Inject(Id = "dev.auros.ditails.version")] Version version)
        {
            _beatSaverInstance = new BeatSaver("DiTails", version.ToString());
        }

        public void LateDispose()
        {
            _beatSaverInstance.Dispose();
        }

        internal async Task<Beatmap?> GetBeatmap(IDifficultyBeatmap difficultyBeatmap, CancellationToken token)
        {
            var hash = difficultyBeatmap.level.levelID.Replace("custom_level_", "");
            if (!_mapCache.TryGetValue(hash, out Beatmap? beatmap))
            {
                beatmap = await _beatSaverInstance.Hash(hash, new StandardRequestOptions { Token = token });
                if (beatmap == null)
                {
                    return null;
                }
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