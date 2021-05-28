using System;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using SiraUtil;
using SiraUtil.Tools;
using UnityEngine.EventSystems;
using Zenject;
using Accessors = DiTails.Utilities.Accessors;

namespace DiTails.Managers
{
    internal sealed class DetailContextManager : IInitializable, IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly ClickableImage _artworkImage;
        private readonly StandardLevelDetailViewController _standardLevelDetailViewController;

        internal event Action? BeatmapUnselected;
        internal event Action<StandardLevelDetailViewController, IDifficultyBeatmap>? DetailMenuRequested;

        #region Initialization

        internal DetailContextManager(SiraLog siraLog, StandardLevelDetailViewController standardLevelDetailViewController)
        {
            _siraLog = siraLog;
            _standardLevelDetailViewController = standardLevelDetailViewController;

            var detailView = Accessors.DetailView(ref standardLevelDetailViewController);
            var levelBar = Accessors.LevelBar(ref detailView);
            var artwork = Accessors.Artwork(ref levelBar);

            // Upgrade the ImageView
            var clickable = artwork.Upgrade<ImageView, ClickableImage>();
            Accessors.Artwork(ref levelBar) = clickable;
            _artworkImage = clickable;
        }

        public void Initialize()
        {
            _artworkImage.OnClickEvent += ArtworkImageClicked;
            _standardLevelDetailViewController.didDeactivateEvent += DetailViewDeactivated;
            _standardLevelDetailViewController.didChangeContentEvent += DetailViewContentChanged;
        }

        public void Dispose()
        {
            _artworkImage.OnClickEvent -= ArtworkImageClicked;
            _standardLevelDetailViewController.didDeactivateEvent -= DetailViewDeactivated;
            _standardLevelDetailViewController.didChangeContentEvent -= DetailViewContentChanged;
        }

        #endregion

        private void ArtworkImageClicked(PointerEventData pointerEventData)
        {
            DetailMenuRequested?.Invoke(_standardLevelDetailViewController, _standardLevelDetailViewController.selectedDifficultyBeatmap);
        }

        private void DetailViewDeactivated(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            BeatmapUnselected?.Invoke();
        }

        private void DetailViewContentChanged(StandardLevelDetailViewController _, StandardLevelDetailViewController.ContentType contentType)
        {
            if (contentType != StandardLevelDetailViewController.ContentType.OwnedAndReady)
            {
                BeatmapUnselected?.Invoke();
            }
        }
    }
}