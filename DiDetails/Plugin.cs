using IPA;
using HMUI;
using SiraUtil;
using SiraUtil.Zenject;
using DiDetails.Installers;
using IPALogger = IPA.Logging.Logger;
using BeatSaberMarkupLanguage.Components;
using Accessors = DiDetails.Utilities.Accessors;

namespace DiDetails
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Log = logger;

            // Register our Installer
            zenjector.OnMenu<DiDMenuInstaller>()
                .Mutate<StandardLevelDetailViewController>((ctx, obj) =>
                {
                    // Get the ImageView reference on the view controller
                    var viewController = obj as StandardLevelDetailViewController;
                    var detailView = Accessors.DetailView(ref viewController);
                    var levelBar = Accessors.LevelBar(ref detailView);
                    var artwork = Accessors.Artwork(ref levelBar);

                    // Upgrade the ImageView
                    var clickable = artwork.Upgrade<ImageView, ClickableImage>();
                    Accessors.Artwork(ref levelBar) = clickable;
                });
        }

        [OnEnable]
        public void OnEnable()
        {

        }

        [OnDisable]
        public void OnDisable()
        {

        }
    }
}