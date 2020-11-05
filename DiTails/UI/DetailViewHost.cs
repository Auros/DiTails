using System;
using Zenject;
using System.IO;
using UnityEngine;
using SiraUtil.Tools;
using DiTails.Managers;
using System.Reflection;
using System.ComponentModel;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using System.Threading;

namespace DiTails.UI
{
    internal class DetailViewHost : INotifyPropertyChanged, IInitializable, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _didParse;
        private bool _didSetupVote;
        private CancellationTokenSource _cts;

        private string? _bsmlContent;
        private readonly SiraLog _siraLog;
        private readonly LevelDataService _levelDataService;
        private readonly DetailContextManager _detailContextManager;

        #region Initialization 

        public DetailViewHost(SiraLog siraLog, LevelDataService levelDataService, DetailContextManager detailContextManager)
        {
            _siraLog = siraLog;
            _levelDataService = levelDataService;
            _detailContextManager = detailContextManager;

            _cts = new CancellationTokenSource();
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
                _siraLog.Debug("Doing Initial BSML Parsing of the Detail View");
                _siraLog.Debug("Getting Manifest Stream");
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DiTails.Views.detail-view.bsml"))
                using (var reader = new StreamReader(stream))
                {
                    _siraLog.Debug("Reading Manifest Stream");
                    _bsmlContent = await reader.ReadToEndAsync();
                }
                if (!string.IsNullOrWhiteSpace(_bsmlContent))
                {
                    _siraLog.Debug("Parsing Details");
                    BSMLParser.instance.Parse(_bsmlContent, standardLevelDetailViewController.gameObject, this);
                    _siraLog.Debug("Parsing Complete");
                    _didParse = true;
                }
            }
        }

        private void SetupVotingButtons()
        {
            if (!_didSetupVote)
            {
                if (votingUpvoteImage != null && votingDownvoteImage != null)
                {
                    votingUpvoteImage.SetImage("DiTails.Resources.arrow.png");
                    votingDownvoteImage.SetImage("DiTails.Resources.arrow.png");
                    votingUpvoteImage.DefaultColor = new Color(0.388f, 1f, 0.388f);
                    votingDownvoteImage.DefaultColor = new Color(1f, 0.188f, 0.188f);

                    votingUpvoteImage.transform.localScale = new Vector2(0.9f, 1f);
                    votingDownvoteImage.transform.localScale = new Vector2(0.9f, -1f);

                    _didSetupVote = true;
                }
            }
        }

        #endregion

        #region Callbacks

        private void HideMenu()
        {
            _cts.Cancel();
            if (_didParse && rootTransform != null && mainModalTransform != null)
            {
                mainModalTransform.transform.SetParent(rootTransform.transform);
            }
            parserParams?.EmitEvent("hide-detail");
        }

        private void MenuRequested(StandardLevelDetailViewController standardLevelDetailViewController, IDifficultyBeatmap difficultyBeatmap)
        {
            _cts = new CancellationTokenSource();
            _ = LoadMenu(standardLevelDetailViewController, difficultyBeatmap);
        }

        #endregion

        private async Task LoadMenu(StandardLevelDetailViewController standardLevelDetailViewController, IDifficultyBeatmap difficultyBeatmap)
        {
            await Parse(standardLevelDetailViewController);
            SetupVotingButtons();

            parserParams?.EmitEvent("show-detail");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("E"));

            var map = await _levelDataService.GetBeatmap(difficultyBeatmap, _cts.Token);
            if (map != null)
            {
                _siraLog.Debug(map.Name);
                _siraLog.Debug(map.Key);
                _siraLog.Debug($"{map.Stats.UpVotes + -map.Stats.DownVotes}");
            }
        }

        #region Usage



        #endregion

        #region BSML Variables

        [UIParams]
        protected BSMLParserParams? parserParams;

        [UIComponent("root")]
        protected RectTransform? rootTransform;

        [UIComponent("main-modal")]
        protected RectTransform? mainModalTransform;

        [UIComponent("voting-upvote-image")]
        protected ClickableImage? votingUpvoteImage;

        [UIComponent("voting-downvote-image")]
        protected ClickableImage? votingDownvoteImage;

        #endregion
    }
}