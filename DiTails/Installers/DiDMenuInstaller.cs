using DiTails.Managers;
using DiTails.UI;
using Zenject;

namespace DiTails.Installers
{
    internal sealed class DiDMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<DetailViewHost>().AsSingle();
            Container.BindInterfacesAndSelfTo<LevelDataService>().AsSingle();
            Container.BindInterfacesAndSelfTo<DetailContextManager>().AsSingle();
        }
    }
}