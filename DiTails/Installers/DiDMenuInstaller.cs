using SemVer;
using Zenject;
using SiraUtil;
using DiTails.UI;
using IPA.Logging;
using DiTails.Managers;

namespace DiTails.Installers
{
    internal sealed class DiDMenuInstaller : Installer<Logger, Version, DiDMenuInstaller>
    {
        private readonly Logger _logger;
        private readonly Version _version;

        internal DiDMenuInstaller(Logger logger, Version version)
        {
            _logger = logger;
            _version = version;
        }

        public override void InstallBindings()
        {
            Container.BindLoggerAsSiraLogger(_logger);

            Container.Bind<Http>().AsSingle();
            Container.BindInterfacesAndSelfTo<DetailViewHost>().AsSingle();
            Container.BindInterfacesAndSelfTo<LevelDataService>().AsSingle();
            Container.BindInterfacesAndSelfTo<DetailContextManager>().AsSingle();
            Container.BindInstance(_version).WithId("dev.auros.ditails.version").AsSingle();
        }
    }
}