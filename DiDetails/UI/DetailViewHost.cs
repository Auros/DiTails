using System;
using Zenject;
using System.IO;
using SiraUtil.Tools;
using System.Reflection;
using DiDetails.Managers;
using System.ComponentModel;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;

namespace DiDetails.UI
{
    internal class DetailViewHost : INotifyPropertyChanged, IInitializable, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _didParse;
        private string _bsmlContent;
        private readonly SiraLog _siraLog;
        private readonly DetailContextManager _detailContextManager;

        #region Initialization 

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

        private async Task Parse(StandardLevelDetailViewController standardLevelDetailViewController)
        {
            if (!_didParse)
            {
                _siraLog.Info("Starting Parsing Detail View BSML. Getting Manifest Stream");
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DiDetails.Views.detail-view.bsml"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    _siraLog.Info("Reading Manifest Stream");
                    _bsmlContent = await reader.ReadToEndAsync();
                }
                if (!string.IsNullOrWhiteSpace(_bsmlContent))
                {
                    _siraLog.Info("Parsing Details");
                    BSMLParser.instance.Parse(_bsmlContent, standardLevelDetailViewController.gameObject, this);
                    _didParse = true;
                }
            }
        }

        #endregion

        #region Callbacks

        private void HideMenu()
        {
            _siraLog.Info($"Hiding Menu");
        }

        private async void MenuRequested(StandardLevelDetailViewController standardLevelDetailViewController, IDifficultyBeatmap difficultyBeatmap)
        {
            _siraLog.Info($"Detail Request Received for {difficultyBeatmap.level.songName}");
            await Parse(standardLevelDetailViewController);
            parserParams?.EmitEvent("show-detail");
        }

        #endregion

        #region BSML Variables

        [UIParams]
        protected BSMLParserParams parserParams;

        #endregion
    }
}