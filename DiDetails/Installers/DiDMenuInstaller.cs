using Zenject;
using SiraUtil;
using IPA.Logging;
using DiDetails.UI;
using DiDetails.Managers;

namespace DiDetails.Installers
{
    internal sealed class DiDMenuInstaller : Installer<Logger, DiDMenuInstaller>
    {
        private readonly Logger _logger;

        internal DiDMenuInstaller(Logger logger)
        {
            _logger = logger;
        }

        public override void InstallBindings()
        {
            Container.BindLoggerAsSiraLogger(_logger);
            Container.BindInterfacesAndSelfTo<DetailViewHost>().AsSingle();
            Container.BindInterfacesAndSelfTo<DetailContextManager>().AsSingle();
        }
    }
}