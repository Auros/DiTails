using System;
using Zenject;
using SiraUtil.Tools;
using DiDetails.Managers;
using System.ComponentModel;

namespace DiDetails.UI
{
    internal sealed class DetailViewHost : INotifyPropertyChanged, IInitializable, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly SiraLog _siraLog;
        private readonly DetailContextManager _detailContextManager;

        public DetailViewHost(SiraLog siraLog, DetailContextManager detailContextManager)
        {
            _siraLog = siraLog;
            _detailContextManager = detailContextManager;
        }

        public void Initialize()
        {
            _detailContextManager.BeatmapUnselected += HideMenu;
            _detailContextManager.DetailMenuRequested += MenuRequested;
        }

        public void Dispose()
        {
            _detailContextManager.BeatmapUnselected -= HideMenu;
            _detailContextManager.DetailMenuRequested -= MenuRequested;
        }

        private void HideMenu()
        {
            _siraLog.Info($"Hiding Menu");
        }

        private void MenuRequested(IDifficultyBeatmap difficultyBeatmap)
        {
            _siraLog.Info($"Detail Request Received for {difficultyBeatmap.level.songName}");
        }
    }
}