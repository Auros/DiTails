using HMUI;
using TMPro;
using System;
using Zenject;
using SiraUtil;
using System.IO;
using UnityEngine;
using IPA.Utilities;
using BeatSaverSharp;
using SiraUtil.Tools;
using System.Threading;
using DiTails.Managers;
using DiTails.Utilities;
using System.Reflection;
using System.ComponentModel;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;

namespace DiTails.UI
{
    internal class DetailViewHost : INotifyPropertyChanged, IInitializable, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _didParse;
        private bool _didSetupVote;
        private Beatmap? _activeBeatSaverMap;
        private CancellationTokenSource _cts;
        private IDifficultyBeatmap? _activeBeatmap;

        private string? _bsmlContent;
        private readonly SiraLog _siraLog;
        private readonly LevelDataService _levelDataService;
        private readonly IPlatformUserModel _platformUserModel;
        private readonly DetailContextManager _detailContextManager;

        #region Initialization 

        public DetailViewHost(SiraLog siraLog, LevelDataService levelDataService, IPlatformUserModel platformUserModel, DetailContextManager detailContextManager)
        {
            _siraLog = siraLog;
            _levelDataService = levelDataService;
            _platformUserModel = platformUserModel;
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
                var info = await _platformUserModel.GetUserInfo();
                CanVote = info.platform == UserInfo.Platform.Steam || info.platform == UserInfo.Platform.Test;

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
                    if (rootTransform != null && mainModalTransform != null)
                    {
                        rootTransform.gameObject.name = "DiTailsDetailView";
                        mainModalTransform.gameObject.name = "DiTailsMainModal";
                    }
                    if (descriptionModalTransform != null && artworkModalTransform != null)
                    {
                        descriptionModalTransform.gameObject.name = "DiTailsDescriptionModal";
                        artworkModalTransform.gameObject.name = "DiTailsArtworkModal";
                    }
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
            if (_didParse && rootTransform != null && mainModalTransform != null && openURLModalTransform != null && levelHashModalTransform != null && descriptionModalTransform != null && artworkModalTransform != null)
            {
                mainModalTransform.transform.SetParent(rootTransform.transform);
                openURLModalTransform.transform.SetParent(rootTransform.transform);
                levelHashModalTransform.transform.SetParent(rootTransform.transform);
                descriptionModalTransform.transform.SetParent(rootTransform.transform);
                artworkModalTransform.transform.SetParent(rootTransform.transform);
            }
            parserParams?.EmitEvent("hide");
        }

        private void MenuRequested(StandardLevelDetailViewController standardLevelDetailViewController, IDifficultyBeatmap difficultyBeatmap)
        {
            _cts = new CancellationTokenSource();
            _ = LoadMenu(standardLevelDetailViewController, difficultyBeatmap);
        }

        #endregion

        #region Usage

        private async Task LoadMenu(StandardLevelDetailViewController standardLevelDetailViewController, IDifficultyBeatmap difficultyBeatmap)
        {
            _activeBeatmap = difficultyBeatmap;
            await Parse(standardLevelDetailViewController);
            SetupVotingButtons();

            ShowPanel = false;
            parserParams?.EmitEvent("show-detail");
            var map = await _levelDataService.GetBeatmap(difficultyBeatmap, _cts.Token);
            ShowPanel = true;
            if (map != null)
            {
                Key = map.Key;
                Mapper = difficultyBeatmap.level.levelAuthorName ?? map.Uploader.Username ?? "Unknown";
                Uploaded = map.Uploaded.ToString("MMMM dd, yyyy");
                Downloads = map.Stats.Downloads.ToString();
                Votes = (map.Stats.UpVotes + -map.Stats.DownVotes).ToString();
                SetRating(map.Stats.Rating);
            }
            Author = difficultyBeatmap.level.songAuthorName;
            _activeBeatSaverMap = map;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCustomLevel)));
        }

        private void SetRating(float value)
        {
            if (rating != null)
            {
                rating.text = string.Format("{0:0%}", value);
                rating.color = Constants.Evaluate(value);
            }
        }

        protected async Task Vote(bool upvote)
        {
            var info = await _platformUserModel.GetUserInfo();
            CanVote = false;
            try
            {
                VoteLoading = true;
                var ticket = await _platformUserModel.GetUserAuthToken();
                if (_activeBeatSaverMap != null && ticket != null)
                {
                    _siraLog.Debug("Starting Vote...");
                    var ticketBytes = Utils.StringToByteArray(ticket.Replace("-", ""));
                    if (upvote)
                    {
                        await _activeBeatSaverMap.VoteUp(info.platformUserId, ticketBytes);
                    }
                    else
                    {
                        await _activeBeatSaverMap.VoteDown(info.platformUserId, ticketBytes);
                    }
                    _siraLog.Debug($"Voted. Upvote? ({upvote})");
                    Votes = (_activeBeatSaverMap.Stats.UpVotes + -_activeBeatSaverMap.Stats.DownVotes).ToString();
                    SetRating(_activeBeatSaverMap.Stats.Rating);
                }
                VoteLoading = false;
            }
            catch (Exception e)
            {
                _siraLog.Error(e.Message);
            }
            CanVote = info.platform == UserInfo.Platform.Steam || info.platform == UserInfo.Platform.Test;
        }

        #endregion

        #region BSML Actions

        [UIAction("upvote")]
        protected async Task Upvote()
        {
            await Vote(true);
        }

        [UIAction("downvote")]
        protected async Task Downvote()
        {
            await Vote(false);
        }

        [UIAction("view-open-beatsaver-url")]
        protected async Task ViewOpenBeatSaverURL()
        {
            parserParams?.EmitEvent("hide");
            await SiraUtil.Utilities.PauseChamp;
            if (_activeBeatSaverMap != null)
            {
                URL = $"https://beatsaver.com/beatmap/{_activeBeatSaverMap.Key}";
            }
            parserParams?.EmitEvent("show-open-url");
        }

        [UIAction("view-level-hash")]
        protected async Task ViewLevelHash()
        {
            parserParams?.EmitEvent("hide");
            await SiraUtil.Utilities.PauseChamp;
            if (_activeBeatmap != null)
            {
                Hash = _activeBeatmap.level.levelID.Replace("custom_level_", "");
            }
            parserParams?.EmitEvent("show-level-hash");
        }

        [UIAction("view-description")]
        protected async Task ViewDescription()
        {
            parserParams?.EmitEvent("hide");
            await SiraUtil.Utilities.PauseChamp;
            parserParams?.EmitEvent("show-description");
            if (_activeBeatSaverMap != null && textPageScrollView != null)
            {
                textPageScrollView.SetText(_activeBeatSaverMap.Description ?? "DITAILS_NODESCRIPTION".LocalizationGetOr("No Description"));
                await SiraUtil.Utilities.PauseChamp;
                textPageScrollView.SetDestinationPosY(0);
                textPageScrollView.ScrollTo(0, false);
                textPageScrollView.RefreshButtons();
                textPageScrollView.SetText(_activeBeatSaverMap.Description ?? "DITAILS_NODESCRIPTION".LocalizationGetOr("No Description"));
            }
        }

        [UIAction("view-artwork")]
        protected async Task ViewArtwork()
        {
            parserParams?.EmitEvent("hide");
            if (artworkImage != null && _activeBeatmap != null)
            {
                var coverImage = await _activeBeatmap.level.GetCoverImageAsync(_cts.Token);
                coverImage.texture.wrapMode = TextureWrapMode.Clamp;
                artworkImage.sprite = coverImage;
            }
            await SiraUtil.Utilities.PauseChamp;
            parserParams?.EmitEvent("show-artwork");
        }

        [UIAction("close-submodal")]
        protected async Task Close()
        {
            parserParams?.EmitEvent("hide");
            await SiraUtil.Utilities.PauseChamp;
            parserParams?.EmitEvent("show-detail");
        }

        [UIAction("open-url")]
        protected async Task OpenURL()
        {
            Application.OpenURL(URL);
            await Close();
        }

        #endregion

        #region BSML Bindings

        [UIValue("custom-level")]
        protected bool IsCustomLevel => _activeBeatSaverMap != null;

        private bool _canVote = false;
        [UIValue("can-vote")]
        protected bool CanVote
        {
            get => _canVote;
            set
            {
                _canVote = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanVote)));
            }
        }

        private bool _voteLoading = false;
        [UIValue("vote-loading")]
        protected bool VoteLoading
        {
            get => _voteLoading;
            set
            {
                _voteLoading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VoteLoading)));
            }
        }

        [UIValue("show-loading")]
        public bool ShowLoading => !ShowPanel;

        private bool _showPanel = false;
        [UIValue("show-panel")]
        protected bool ShowPanel
        {
            get => _showPanel;
            set
            {
                _showPanel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowPanel)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowLoading)));
            }
        }

        private string _key = "Key | 0";
        [UIValue("key")]
        protected string Key
        {
            get => _key;
            set
            {
                _key = string.Join(" | ", "DITAILS_KEY".LocalizationGetOr("Key"), value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Key)));
            }
        }

        private string _author = "Author | Unknown";
        [UIValue("author")]
        protected string Author
        {
            get => _author;
            set
            {
                _author = string.Join(" | ", "DITAILS_AUTHOR".LocalizationGetOr("Author"), value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Author)));
            }
        }

        private string _mapper = "Mapper | None";
        [UIValue("mapper")]
        protected string Mapper
        {
            get => _mapper;
            set
            {
                _mapper = string.Join(" | ", "DITAILS_MAPPER".LocalizationGetOr("Mapper"), value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Mapper)));
            }
        }

        private string _uploaded = "Uploaded | Never";
        [UIValue("uploaded")]
        protected string Uploaded
        {
            get => _uploaded;
            set
            {
                _uploaded = string.Join(" | ", "DITAILS_UPLOADED".LocalizationGetOr("Uploaded"), value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Uploaded)));
            }
        }

        private string _downloads = "Downloads | 0";
        [UIValue("downloads")]
        protected string Downloads
        {
            get => _downloads;
            set
            {
                _downloads = string.Join(" | ", "DITAILS_DOWNLOADS".LocalizationGetOr("Downloads"), value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Downloads)));
            }
        }

        private string _votes = "0";
        [UIValue("votes")]
        protected string Votes
        {
            get => _votes;
            set
            {
                _votes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Votes)));
            }
        }

        [UIValue("url")]
        protected string URLView => string.Format("DITAILS_OPENURL".LocalizationGetOr("This will open <color=#1159cf>{0}</color> in your browser. Are you sure?"), new Uri(URL).Host);

        private string _url = "https://google.com";
        protected string URL
        {
            get => _url;
            set
            {
                _url = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(URL)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(URLView)));
            }
        }

        private string _hash = "DITAILS_NOHASH".LocalizationGetOr("No Level Hash");
        [UIValue("hash")]
        protected string Hash
        {
            get => _hash;
            set
            {
                _hash = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Hash)));
            }
        }

        #endregion

        #region BSML Variables

        [UIParams]
        protected BSMLParserParams? parserParams;

        [UIComponent("rating")]
        protected TextMeshProUGUI? rating;

        [UIComponent("artwork-image")]
        protected ImageView? artworkImage;

        [UIComponent("root")]
        protected RectTransform? rootTransform;

        [UIComponent("main-modal")]
        protected RectTransform? mainModalTransform;

        [UIComponent("open-url-modal")]
        protected RectTransform? openURLModalTransform;

        [UIComponent("level-hash-modal")]
        protected RectTransform? levelHashModalTransform;

        [UIComponent("description-scroller")]
        protected TextPageScrollView? textPageScrollView;

        [UIComponent("description-modal")]
        protected RectTransform? descriptionModalTransform;

        [UIComponent("artwork-modal")]
        protected RectTransform? artworkModalTransform;

        [UIComponent("voting-upvote-image")]
        protected ClickableImage? votingUpvoteImage;

        [UIComponent("voting-downvote-image")]
        protected ClickableImage? votingDownvoteImage;

        #endregion
    }
}