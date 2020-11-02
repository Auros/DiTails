using System;
using Zenject;
using System.IO;
using UnityEngine;
using SiraUtil.Tools;
using System.Reflection;
using DiDetails.Managers;
using System.ComponentModel;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;

namespace DiDetails.UI
{
    internal class DetailViewHost : INotifyPropertyChanged, IInitializable, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _didParse;
        private bool _didSetupVote;

        private string? _bsmlContent;
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
                _siraLog.Debug("Doing Initial BSML Parsing of the Detail View");
                _siraLog.Debug("Getting Manifest Stream");
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DiDetails.Views.detail-view.bsml"))
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
                    votingUpvoteImage.SetImage("DiDetails.Resources.arrow.png");
                    votingDownvoteImage.SetImage("DiDetails.Resources.arrow.png");
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
            _siraLog.Info("Hiding Menu");
            if (_didParse && rootTransform != null && mainModalTransform != null)
            {
                mainModalTransform.transform.SetParent(rootTransform.transform);
            }
            parserParams?.EmitEvent("hide-detail");
        }

        private async void MenuRequested(StandardLevelDetailViewController standardLevelDetailViewController, IDifficultyBeatmap difficultyBeatmap)
        {
            await Parse(standardLevelDetailViewController);
            SetupVotingButtons();

            parserParams?.EmitEvent("show-detail");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("E"));
        }

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