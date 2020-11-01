using System;
using Zenject;
using SiraUtil.Tools;
using DiDetails.Utilities;
using UnityEngine.EventSystems;
using BeatSaberMarkupLanguage.Components;

namespace DiDetails.Managers
{
    internal sealed class DetailContextManager : IInitializable, IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly ClickableImage _artworkImage;
        private readonly StandardLevelDetailViewController _standardLevelDetailViewController;

        #region Initialization

        internal DetailContextManager(SiraLog siraLog, StandardLevelDetailViewController standardLevelDetailViewController)
        {
            _siraLog = siraLog;
            _standardLevelDetailViewController = standardLevelDetailViewController;

            // Let's ref the image that we already upgraded in our Zenjector
            var detailView = Accessors.DetailView(ref _standardLevelDetailViewController);
            var levelBar = Accessors.LevelBar(ref detailView);
            _artworkImage = (ClickableImage)Accessors.Artwork(ref levelBar);
        }

        public void Initialize()
        {
            _artworkImage.OnClickEvent += ArtworkImageClicked;
        }

        public void Dispose()
        {
            _artworkImage.OnClickEvent -= ArtworkImageClicked;
        }

        #endregion

        private void ArtworkImageClicked(PointerEventData pointerEventData)
        {
            _siraLog.Info("Clicked Image");
        }
    }
}