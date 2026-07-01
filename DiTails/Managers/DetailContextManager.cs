using BeatSaberMarkupLanguage.Components;
using DiTails.Utilities;
using HMUI;
using System;
using UnityEngine.EventSystems;
using Zenject;
using Accessors = DiTails.Utilities.Accessors;

namespace DiTails.Managers
{
    internal sealed class DetailContextManager : IInitializable, IDisposable
    {
        private ClickableImage? _artworkImage;
        private readonly StandardLevelDetailViewController _standardLevelDetailViewController;

        internal event Action? BeatmapUnselected;
        internal event Action<StandardLevelDetailViewController, BeatmapLevel>? DetailMenuRequested;

        #region Initialization

        internal DetailContextManager(StandardLevelDetailViewController standardLevelDetailViewController)
        {
            _standardLevelDetailViewController = standardLevelDetailViewController;
        }

        public void Initialize()
        {
            _artworkImage = GetOrUpgradeArtworkImage();
            _artworkImage.OnClickEvent += ArtworkImageClicked;
            _standardLevelDetailViewController.didDeactivateEvent += DetailViewDeactivated;
            _standardLevelDetailViewController.didChangeContentEvent += DetailViewContentChanged;
        }

        public void Dispose()
        {
            if (_artworkImage != null)
            {
                _artworkImage.OnClickEvent -= ArtworkImageClicked;
            }

            _standardLevelDetailViewController.didDeactivateEvent -= DetailViewDeactivated;
            _standardLevelDetailViewController.didChangeContentEvent -= DetailViewContentChanged;
        }

        #endregion

        private void ArtworkImageClicked(PointerEventData pointerEventData)
        {
            DetailMenuRequested?.Invoke(_standardLevelDetailViewController, _standardLevelDetailViewController.beatmapLevel);
        }

        private ClickableImage GetOrUpgradeArtworkImage()
        {
            var standardLevelDetailViewController = _standardLevelDetailViewController;
            var detailView = Accessors.DetailView(ref standardLevelDetailViewController);
            var levelBar = Accessors.LevelBar(ref detailView);
            var artwork = Accessors.Artwork(ref levelBar);

            if (artwork is ClickableImage clickable)
            {
                return clickable;
            }

            clickable = artwork.Upgrade<ImageView, ClickableImage>();
            Accessors.Artwork(ref levelBar) = clickable;
            return clickable;
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
