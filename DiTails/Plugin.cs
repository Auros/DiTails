using DiTails.Installers;
using IPA;
using IPA.Loader;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

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
            zenjector.OnMenu<DiDMenuInstaller>().WithParameters(logger, metadata.HVersion);
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