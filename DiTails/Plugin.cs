using IPA;
using HMUI;
using SiraUtil;
using IPA.Loader;
using SiraUtil.Zenject;
using DiTails.Installers;
using SiraUtil.Attributes;
using IPALogger = IPA.Logging.Logger;
using BeatSaberMarkupLanguage.Components;
using Accessors = DiTails.Utilities.Accessors;

namespace DiTails
{
    [Plugin(RuntimeOptions.DynamicInit), Slog]
    public class Plugin
    {
        internal static IPALogger? Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector, PluginMetadata metadata)
        {
            Log = logger;
            zenjector
                .On<PCAppInit>()
                .Pseudo(Container => Container.BindInstance(new UBinder<Plugin, PluginMetadata>(metadata)));

            // Register our Installer
            zenjector.OnMenu<DiDMenuInstaller>()
                .Mutate<StandardLevelDetailViewController>((ctx, obj) =>
                {
                    // Get the ImageView reference on the view controller
                    if (obj is StandardLevelDetailViewController viewController)
                    {
                        var detailView = Accessors.DetailView(ref viewController);
                        var levelBar = Accessors.LevelBar(ref detailView);
                        var artwork = Accessors.Artwork(ref levelBar);

                        // Upgrade the ImageView
                        var clickable = artwork.Upgrade<ImageView, ClickableImage>();
                        Accessors.Artwork(ref levelBar) = clickable;
                    }
                })
                .WithParameters(logger, metadata.Version);
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