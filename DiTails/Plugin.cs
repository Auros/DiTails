using DiTails.Installers;
using IPA;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace DiTails
{
    [Plugin(RuntimeOptions.DynamicInit), Slog, NoEnableDisable]
    public class Plugin
    {
        internal static IPALogger? Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Log = logger;
            zenjector.UseMetadataBinder<Plugin>();

            // Register our Installer
            zenjector.Install<DiDMenuInstaller>(Location.Menu);
            zenjector.UseLogger(logger);
        }
    }
}